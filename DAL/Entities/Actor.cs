using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace DAL.Entities;

public class Actor : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(ActorConstraints.FirstNameMaxLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(ActorConstraints.LastNameMaxLength)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(ActorConstraints.NationalityMaxLength)]
    public string? Nationality { get; set; }

    [StringLength(ActorConstraints.BiographyMaxLength)]
    public string? Biography { get; set; }

    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
}
