using System.ComponentModel.DataAnnotations;
using DAL.Constraints;
using Microsoft.EntityFrameworkCore;

namespace DAL.Entities;

public class Movie : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(MovieConstraints.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(MovieConstraints.GenreMaxLength)]
    public string Genre { get; set; } = string.Empty;

    [Range(MovieConstraints.MinReleaseYear, MovieConstraints.MaxReleaseYear)]
    public int ReleaseYear { get; set; }

    [Range(MovieConstraints.MinDurationMinutes, MovieConstraints.MaxDurationMinutes)]
    public int DurationMinutes { get; set; }

    [StringLength(MovieConstraints.DirectorMaxLength)]
    public string? Director { get; set; }

    [Range(MovieConstraints.MinRating, MovieConstraints.MaxRating)]
    [Precision(MovieConstraints.RatingPrecision, MovieConstraints.RatingScale)]
    public decimal? Rating { get; set; }

    [StringLength(MovieConstraints.DescriptionMaxLength)]
    public string? Description { get; set; }

    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
