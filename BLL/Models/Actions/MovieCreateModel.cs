using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Actions;

public class MovieCreateModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Genre { get; set; } = string.Empty;

    [Required]
    [Range(1888, 2100)]
    [Display(Name = "Release year")]
    public int? ReleaseYear { get; set; }

    [Required]
    [Range(1, 600)]
    [Display(Name = "Duration (minutes)")]
    public int? DurationMinutes { get; set; }

    [StringLength(150)]
    public string? Director { get; set; }

    [Range(0.0, 10.0)]
    [Display(Name = "Rating (0-10)")]
    public decimal? Rating { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Actors")]
    public List<Guid> ActorIds { get; set; } = [];
}
