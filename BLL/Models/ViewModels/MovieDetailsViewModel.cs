namespace BLL.Models.ViewModels;

public class MovieDetailsViewModel : MovieViewModel
{
    public List<CastMemberViewModel> Cast { get; set; } = [];
}
