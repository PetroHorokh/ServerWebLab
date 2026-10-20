using BLL.Models.ViewModels;

namespace BLL.Services.Interfaces;

public interface IChatNotifier
{
    Task MessagePostedAsync(ChatMessageViewModel message, IReadOnlyCollection<Guid> privateAudience, CancellationToken cancellationToken = default);

    Task MessageDeletedAsync(Guid movieId, Guid messageId, IReadOnlyCollection<Guid> privateAudience, CancellationToken cancellationToken = default);
}
