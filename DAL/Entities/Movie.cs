namespace DAL.Entities;

public class Movie : AuditableEntity
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public int DurationMinutes { get; set; }

    public string? Director { get; set; }

    public decimal? Rating { get; set; }

    public string? Description { get; set; }

    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
