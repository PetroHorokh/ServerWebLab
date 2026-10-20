using BLL.Models.ViewModels;

namespace Web.Hubs;

public interface IChatClient
{
    Task MessagePosted(ChatMessageViewModel message);

    Task MessageDeleted(Guid movieId, Guid messageId);
}
