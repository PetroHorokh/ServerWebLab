using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Actions;

public class ActorCreateModel : IValidatableObject
{
    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [StringLength(100)]
    public string? Nationality { get; set; }

    [StringLength(2000)]
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
