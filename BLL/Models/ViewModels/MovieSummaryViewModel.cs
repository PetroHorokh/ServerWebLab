using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class MovieSummaryViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(MovieConstraints.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [Range(MovieConstraints.MinReleaseYear, MovieConstraints.MaxReleaseYear)]
    [Display(Name = "Release year")]
    public int ReleaseYear { get; set; }

    public string DisplayName => $"{Title} ({ReleaseYear})";
}
