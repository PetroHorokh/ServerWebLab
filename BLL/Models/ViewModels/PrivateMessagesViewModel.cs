namespace BLL.Models.ViewModels;

public class PrivateMessagesViewModel
{
    public List<ChatMessageViewModel> Messages { get; set; } = [];

    public List<PrivateMessageMovieViewModel> Movies { get; set; } = [];

    public Guid? SelectedMovieId { get; set; }

    public int TotalCount { get; set; }

    public int Limit { get; set; }
}
