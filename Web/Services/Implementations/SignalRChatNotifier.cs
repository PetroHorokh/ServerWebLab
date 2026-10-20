using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services.Implementations;

public sealed class SignalRChatNotifier(IHubContext<ChatHub, IChatClient> hubContext) : IChatNotifier
{
    public Task MessagePostedAsync(ChatMessageViewModel message, IReadOnlyCollection<Guid> privateAudience, CancellationToken cancellationToken = default) =>
        message.IsPrivate
            ? hubContext.Clients.Users(ToUserIds(privateAudience)).MessagePosted(message)
            : hubContext.Clients.Group(ChatHub.MovieGroup(message.MovieId)).MessagePosted(message);

    public async Task MessageDeletedAsync(Guid movieId, Guid messageId, IReadOnlyCollection<Guid> privateAudience, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.Group(ChatHub.MovieGroup(movieId)).MessageDeleted(movieId, messageId);

        if (privateAudience.Count > 0)
        {
            await hubContext.Clients.Users(ToUserIds(privateAudience)).MessageDeleted(movieId, messageId);
        }
    }

    private static List<string> ToUserIds(IEnumerable<Guid> userIds) =>
        userIds.Select(id => id.ToString()).ToList();
}
