using BLL.Models.ViewModels;

namespace Web.Models;

public class MovieDetailsPageViewModel
{
    public required MovieDetailsViewModel Movie { get; init; }

    public required IReadOnlyList<ChatMessageViewModel> Messages { get; init; }

    public required DateTime ChatSyncedAt { get; init; }
}
