using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

public sealed class ChatHub(IChatService chatService) : Hub<IChatClient>
{
    public const string Path = "/hubs/chat";

    private const string ChatClosedMessage = "This movie no longer exists, so its chat is closed.";

    public static string MovieGroup(Guid movieId) => $"movie:{movieId}";

    public async Task<ChatUpdatesViewModel?> JoinMovie(Guid movieId, Guid? afterId, DateTime? since)
    {
        if (!await chatService.MovieExistsAsync(movieId, Context.ConnectionAborted))
        {
            return null;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, MovieGroup(movieId), Context.ConnectionAborted);
        return await chatService.GetUpdatesAsync(movieId, afterId, since, Context.ConnectionAborted);
    }

    public Task LeaveMovie(Guid movieId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, MovieGroup(movieId), Context.ConnectionAborted);

    [Authorize(Roles = AppRoles.AdminOrMember)]
    public async Task<ChatSendResponse> SendMessage(Guid movieId, string? message, bool isPrivate)
    {
        var input = new ChatMessageCreateModel { Message = message, IsPrivate = isPrivate };
        var result = await chatService.SendAsync(movieId, input, [], Context.ConnectionAborted);

        return result.Status switch
        {
            ServiceResultStatus.Success => new ChatSendResponse(result.Value, null, false),
            ServiceResultStatus.NotFound => new ChatSendResponse(null, ChatClosedMessage, true),
            ServiceResultStatus.Forbidden => new ChatSendResponse(null, result.Error, true),
            _ => new ChatSendResponse(null, result.Error, false),
        };
    }
}
