using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;

namespace BLL.Services.Interfaces;

public interface IChatService
{
    Task<bool> MovieExistsAsync(Guid movieId, CancellationToken cancellationToken = default);

    Task<ChatUpdatesViewModel> GetUpdatesAsync(Guid movieId, Guid? afterMessageId, DateTime? sinceUtc, CancellationToken cancellationToken = default);

    Task<ChatMessageViewModel?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResult<ChatMessageViewModel>> SendAsync(
        Guid movieId,
        ChatMessageCreateModel model,
        IReadOnlyList<ChatAttachmentUploadModel> attachments,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PrivateMessagesViewModel?> GetPrivateMessagesAsync(Guid? movieId, CancellationToken cancellationToken = default);

    Task<ChatAttachmentFileViewModel?> GetAttachmentAsync(Guid id, CancellationToken cancellationToken = default);
}
