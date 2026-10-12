using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.Actions;

public class RegisterModel : IValidatableObject
{
    [Required]
    [StringLength(UserIdentityConstraints.DisplayNameMaxLength, MinimumLength = UserIdentityConstraints.DisplayNameMinLength)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(UserIdentityConstraints.FirstNameMaxLength)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(UserIdentityConstraints.LastNameMaxLength)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(UserConstraints.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(UserConstraints.PasswordMaxLength, MinimumLength = UserConstraints.PasswordMinLength)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords don't match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult("Date of birth cannot be in the future.", [nameof(DateOfBirth)]);
        }
        else if (DateOfBirth < new DateOnly(1900, 1, 1))
        {
            yield return new ValidationResult("Enter a date of birth after 1900.", [nameof(DateOfBirth)]);
        }
    }
}
