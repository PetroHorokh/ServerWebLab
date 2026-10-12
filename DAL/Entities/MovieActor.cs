namespace DAL.Entities;

public class MovieActor : AuditableEntity
{
    public Guid MovieId { get; set; }

    public Guid ActorId { get; set; }

    public string? CharacterName { get; set; }

    public bool IsLeadRole { get; set; }

    public int? BillingOrder { get; set; }

    public Movie Movie { get; set; } = null!;

    public Actor Actor { get; set; } = null!;
}
