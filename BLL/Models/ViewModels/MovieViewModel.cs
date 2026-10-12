using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class MovieViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(MovieConstraints.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(MovieConstraints.GenreMaxLength)]
    public string Genre { get; set; } = string.Empty;

    [Range(MovieConstraints.MinReleaseYear, MovieConstraints.MaxReleaseYear)]
    [Display(Name = "Release year")]
    public int ReleaseYear { get; set; }

    [Range(MovieConstraints.MinDurationMinutes, MovieConstraints.MaxDurationMinutes)]
    [Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; }

    [StringLength(MovieConstraints.DirectorMaxLength)]
    public string? Director { get; set; }

    [Range(MovieConstraints.MinRating, MovieConstraints.MaxRating)]
    [Display(Name = "Rating (0-10)")]
    public decimal? Rating { get; set; }

    [StringLength(MovieConstraints.DescriptionMaxLength)]
    public string? Description { get; set; }

    [Display(Name = "Actors")]
    public List<ActorSummaryViewModel> Actors { get; set; } = [];

    public string DisplayName => $"{Title} ({ReleaseYear})";
}
