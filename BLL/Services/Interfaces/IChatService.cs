using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;

namespace BLL.Services.Interfaces;

public interface IChatService
{
    Task<ChatUpdatesViewModel> GetUpdatesAsync(Guid movieId, Guid? afterMessageId, DateTime? sinceUtc, CancellationToken cancellationToken = default);

    Task<ChatMessageViewModel?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ChatMessageViewModel?> SendAsync(Guid movieId, ChatMessageCreateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
