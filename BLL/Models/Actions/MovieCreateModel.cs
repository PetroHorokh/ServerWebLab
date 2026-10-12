using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.Actions;

public class MovieCreateModel
{
    [Required]
    [StringLength(MovieConstraints.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(MovieConstraints.GenreMaxLength)]
    public string Genre { get; set; } = string.Empty;

    [Required]
    [Range(MovieConstraints.MinReleaseYear, MovieConstraints.MaxReleaseYear)]
    [Display(Name = "Release year")]
    public int? ReleaseYear { get; set; }

    [Required]
    [Range(MovieConstraints.MinDurationMinutes, MovieConstraints.MaxDurationMinutes)]
    [Display(Name = "Duration (minutes)")]
    public int? DurationMinutes { get; set; }

    [StringLength(MovieConstraints.DirectorMaxLength)]
    public string? Director { get; set; }

    [Range(MovieConstraints.MinRating, MovieConstraints.MaxRating)]
    [Display(Name = "Rating (0-10)")]
    public decimal? Rating { get; set; }

    [StringLength(MovieConstraints.DescriptionMaxLength)]
    public string? Description { get; set; }

    [Display(Name = "Actors")]
    public List<Guid> ActorIds { get; set; } = [];
}
