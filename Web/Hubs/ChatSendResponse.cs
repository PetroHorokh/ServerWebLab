using BLL.Models.ViewModels;

namespace Web.Hubs;

public sealed record ChatSendResponse(ChatMessageViewModel? Message, string? Error, bool ChatClosed);
