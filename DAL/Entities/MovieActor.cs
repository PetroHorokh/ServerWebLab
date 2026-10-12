using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace DAL.Entities;

public class MovieActor : AuditableEntity
{
    public Guid MovieId { get; set; }

    public Guid ActorId { get; set; }

    [StringLength(MovieActorConstraints.CharacterNameMaxLength)]
    public string? CharacterName { get; set; }

    public bool IsLeadRole { get; set; }

    [Range(MovieActorConstraints.MinBillingOrder, MovieActorConstraints.MaxBillingOrder)]
    public int? BillingOrder { get; set; }

    public Movie Movie { get; set; } = null!;

    public Actor Actor { get; set; } = null!;
}
