using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class PrivateMessageMovieViewModel
{
    public Guid MovieId { get; set; }

    [Required]
    [StringLength(MovieConstraints.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    public int Count { get; set; }
}
