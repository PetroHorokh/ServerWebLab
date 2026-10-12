using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.Actions;

public class ActorCreateModel : IValidatableObject
{
    [Required]
    [StringLength(ActorConstraints.FirstNameMaxLength)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(ActorConstraints.LastNameMaxLength)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [StringLength(ActorConstraints.NationalityMaxLength)]
    public string? Nationality { get; set; }

    [StringLength(ActorConstraints.BiographyMaxLength)]
    public string? Biography { get; set; }

    [Display(Name = "Movies")]
    public List<Guid> MovieIds { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult("Date of birth cannot be in the future.", [nameof(DateOfBirth)]);
        }
    }
}
