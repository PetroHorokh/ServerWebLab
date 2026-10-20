using System.ComponentModel.DataAnnotations;
using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services.Implementations;

internal sealed class ChatService(
    IChatMessageRepository chatMessageRepository,
    IChatAttachmentRepository chatAttachmentRepository,
    IMovieRepository movieRepository,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser,
    IChatNotifier chatNotifier,
    AuditStamper auditStamper,
    TimeProvider timeProvider,
    IMapper mapper) : IChatService
{
    public const int HistorySize = 50;

    public const int PrivateInboxSize = 200;

    private const string NoLongerAllowedMessage = "You can't write in this chat any more. Reload the page to see your current access.";

    private static readonly TimeSpan SyncOverlap = TimeSpan.FromSeconds(5);

    public Task<bool> MovieExistsAsync(Guid movieId, CancellationToken cancellationToken = default) =>
        movieRepository.ExistsAsync(movieId, cancellationToken);

    public async Task<ChatUpdatesViewModel> GetUpdatesAsync(
        Guid movieId,
        Guid? afterMessageId,
        DateTime? sinceUtc,
        CancellationToken cancellationToken = default)
    {
        var syncedAt = timeProvider.GetUtcNow().UtcDateTime;
        var viewer = await GetViewerAsync();

        var messages = afterMessageId is { } lastSeenId
            ? await chatMessageRepository.GetForMovieAfterAsync(movieId, lastSeenId, viewer.UserId, viewer.IsAdmin, cancellationToken)
            : await chatMessageRepository.GetLatestForMovieAsync(movieId, HistorySize, viewer.UserId, viewer.IsAdmin, cancellationToken);

        var removedIds = sinceUtc is { } since && since > DateTime.MinValue + SyncOverlap
            ? await chatMessageRepository.GetDeletedIdsForMovieSinceAsync(movieId, since - SyncOverlap, cancellationToken)
            : [];

        return new ChatUpdatesViewModel
        {
            Messages = mapper.Map<List<ChatMessageViewModel>>(messages),
            RemovedMessageIds = removedIds,
            SyncedAt = syncedAt,
        };
    }

    public async Task<ChatMessageViewModel?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await chatMessageRepository.GetWithAuthorAsync(id, cancellationToken);
        if (message is null || message.IsDeleted)
        {
            return null;
        }

        var viewer = await GetViewerAsync();
        return viewer.CanSee(message) ? mapper.Map<ChatMessageViewModel>(message) : null;
    }

    public async Task<ServiceResult<ChatMessageViewModel>> SendAsync(
        Guid movieId,
        ChatMessageCreateModel model,
        IReadOnlyList<ChatAttachmentUploadModel> attachments,
        CancellationToken cancellationToken = default)
    {
        var viewer = await GetViewerAsync();
        if (viewer is not { UserId: { } authorId, RoleTag: { } roleTag })
        {
            return ServiceResult<ChatMessageViewModel>.Forbidden(NoLongerAllowedMessage);
        }

        if (!await movieRepository.ExistsAsync(movieId, cancellationToken))
        {
            return ServiceResult<ChatMessageViewModel>.NotFound();
        }

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), validationResults, validateAllProperties: true))
        {
            return ServiceResult<ChatMessageViewModel>.Invalid(validationResults[0].ErrorMessage ?? "The message is not valid.");
        }

        if (string.IsNullOrWhiteSpace(model.Message) && attachments.Count == 0)
        {
            return ServiceResult<ChatMessageViewModel>.Invalid("Write a message or attach a file.");
        }

        var (fileError, files) = await ChatAttachmentRules.ReadAsync(attachments, cancellationToken);
        if (fileError is not null)
        {
            return ServiceResult<ChatMessageViewModel>.Invalid(fileError);
        }

        var message = auditStamper.StampCreated(mapper.Map<ChatMessage>(model), authorId);
        message.MovieId = movieId;
        message.UserId = authorId;
        message.RoleTag = roleTag;
        message.PostedDateTime = message.CreatedDateTime;

        foreach (var file in files)
        {
            message.Attachments.Add(auditStamper.StampCreated(
                new ChatAttachment
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Content.Length,
                    Data = new ChatAttachmentData { Content = file.Content },
                },
                authorId));
        }

        await chatMessageRepository.AddAsync(message, cancellationToken);
        await chatMessageRepository.SaveChangesAsync(cancellationToken);

        var saved = mapper.Map<ChatMessageViewModel>(await chatMessageRepository.GetWithAuthorAsync(message.Id, CancellationToken.None));
        var audience = saved.IsPrivate ? await GetPrivateAudienceAsync(authorId) : [];
        await chatNotifier.MessagePostedAsync(saved, audience, CancellationToken.None);

        return ServiceResult<ChatMessageViewModel>.Success(saved);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var viewer = await GetViewerAsync();
        if (!viewer.IsAdmin)
        {
            return ServiceResult.Forbidden("Only admins can delete messages.");
        }

        var message = await chatMessageRepository.GetByIdAsync(id, cancellationToken);
        if (message is null || message.IsDeleted)
        {
            return ServiceResult.NotFound();
        }

        message.IsDeleted = true;
        auditStamper.StampModified(message);
        await chatMessageRepository.SaveChangesAsync(cancellationToken);

        var audience = message.IsPrivate ? await GetPrivateAudienceAsync(message.UserId) : [];
        await chatNotifier.MessageDeletedAsync(message.MovieId, message.Id, audience, CancellationToken.None);

        return ServiceResult.Success();
    }

    public async Task<PrivateMessagesViewModel?> GetPrivateMessagesAsync(Guid? movieId, CancellationToken cancellationToken = default)
    {
        var viewer = await GetViewerAsync();
        if (!viewer.IsAdmin)
        {
            return null;
        }

        var movies = await chatMessageRepository.GetPrivateCountsByMovieAsync(cancellationToken);
        var messages = await chatMessageRepository.GetLatestPrivateAsync(movieId, PrivateInboxSize, cancellationToken);

        return new PrivateMessagesViewModel
        {
            Messages = mapper.Map<List<ChatMessageViewModel>>(messages),
            Movies = movies
                .Select(m => new PrivateMessageMovieViewModel { MovieId = m.MovieId, Title = m.Title, Count = m.Count })
                .ToList(),
            SelectedMovieId = movieId,
            TotalCount = movies.Where(m => movieId is null || m.MovieId == movieId).Sum(m => m.Count),
            Limit = PrivateInboxSize,
        };
    }

    public async Task<ChatAttachmentFileViewModel?> GetAttachmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attachment = await chatAttachmentRepository.GetWithContentAsync(id, cancellationToken);
        if (attachment is null || attachment.ChatMessage.IsDeleted)
        {
            return null;
        }

        if (attachment.ChatMessage.IsPrivate && !(await GetViewerAsync()).CanSee(attachment.ChatMessage))
        {
            return null;
        }

        return new ChatAttachmentFileViewModel
        {
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            Content = attachment.Data.Content,
        };
    }

    private async Task<ChatViewer> GetViewerAsync()
    {
        if (!currentUser.IsAuthenticated)
        {
            return ChatViewer.Guest;
        }

        var user = await userManager.FindByIdAsync(currentUser.UserId.ToString());
        if (user is null || AccountLockout.IsLockedByAdmin(user.LockoutEnd))
        {
            return ChatViewer.Guest;
        }

        var roles = await userManager.GetRolesAsync(user);
        var roleTag = roles.Contains(AppRoles.Admin) ? AppRoles.Admin
            : roles.Contains(AppRoles.Member) ? AppRoles.Member
            : null;

        return new ChatViewer(user.Id, roleTag);
    }

    private async Task<IReadOnlyCollection<Guid>> GetPrivateAudienceAsync(Guid authorId)
    {
        var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
        return admins
            .Where(admin => !AccountLockout.IsLockedByAdmin(admin.LockoutEnd))
            .Select(admin => admin.Id)
            .Append(authorId)
            .Distinct()
            .ToList();
    }

    private sealed record ChatViewer(Guid? UserId, string? RoleTag)
    {
        public static ChatViewer Guest { get; } = new(null, null);

        public bool IsAdmin => RoleTag == AppRoles.Admin;

        public bool CanSee(ChatMessage message) => !message.IsPrivate || IsAdmin || message.UserId == UserId;
    }
}
