using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations;

internal sealed class ChatService(
    IChatMessageRepository chatMessageRepository,
    IMovieRepository movieRepository,
    ICurrentUserService currentUser,
    AuditStamper auditStamper,
    TimeProvider timeProvider,
    IMapper mapper) : IChatService
{
    public const int HistorySize = 50;

    private static readonly TimeSpan SyncOverlap = TimeSpan.FromSeconds(5);

    public async Task<ChatUpdatesViewModel> GetUpdatesAsync(
        Guid movieId,
        Guid? afterMessageId,
        DateTime? sinceUtc,
        CancellationToken cancellationToken = default)
    {
        var syncedAt = timeProvider.GetUtcNow().UtcDateTime;

        var messages = afterMessageId is { } lastSeenId
            ? await chatMessageRepository.GetForMovieAfterAsync(movieId, lastSeenId, cancellationToken)
            : await chatMessageRepository.GetLatestForMovieAsync(movieId, HistorySize, cancellationToken);

        var removedIds = sinceUtc is { } since
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
        return message is null || message.IsDeleted ? null : mapper.Map<ChatMessageViewModel>(message);
    }

    public async Task<ChatMessageViewModel?> SendAsync(Guid movieId, ChatMessageCreateModel model, CancellationToken cancellationToken = default)
    {
        if (!await movieRepository.ExistsAsync(movieId, cancellationToken))
        {
            return null;
        }

        var message = auditStamper.StampCreated(mapper.Map<ChatMessage>(model));
        message.MovieId = movieId;
        message.UserId = currentUser.UserId;
        message.RoleTag = currentUser.IsInRole(AppRoles.Admin) ? AppRoles.Admin : AppRoles.Member;
        message.PostedDateTime = message.CreatedDateTime;

        await chatMessageRepository.AddAsync(message, cancellationToken);
        await chatMessageRepository.SaveChangesAsync(cancellationToken);

        var saved = await chatMessageRepository.GetWithAuthorAsync(message.Id, cancellationToken);
        return mapper.Map<ChatMessageViewModel>(saved);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await chatMessageRepository.GetByIdAsync(id, cancellationToken);
        if (message is null || message.IsDeleted)
        {
            return ServiceResult.NotFound();
        }

        message.IsDeleted = true;
        auditStamper.StampModified(message);
        await chatMessageRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
