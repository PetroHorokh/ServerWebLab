namespace BLL.Models.ViewModels;

public class ChatUpdatesViewModel
{
    public List<ChatMessageViewModel> Messages { get; set; } = [];

    public List<Guid> RemovedMessageIds { get; set; } = [];

    public DateTime SyncedAt { get; set; }
}
