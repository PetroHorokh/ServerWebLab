using System.ComponentModel.DataAnnotations;

namespace BLL.Models.ViewModels;

public class MovieDetailsViewModel : MovieViewModel
{
    [Display(Name = "Cast")]
    public List<CastMemberViewModel> Cast { get; set; } = [];
}
