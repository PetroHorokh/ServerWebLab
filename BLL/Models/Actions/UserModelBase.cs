using System.ComponentModel.DataAnnotations;
using BLL.Common;
using DAL.Constraints;

namespace BLL.Models.Actions;

public abstract class UserModelBase : IValidatableObject
{
    [Required]
    [StringLength(UserIdentityConstraints.FirstNameMaxLength)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(UserIdentityConstraints.LastNameMaxLength)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(UserIdentityConstraints.DisplayNameMaxLength, MinimumLength = UserIdentityConstraints.DisplayNameMinLength)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(UserConstraints.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [AllowedValues(AppRoles.Admin, AppRoles.Member, ErrorMessage = "Select a valid role.")]
    public string Role { get; set; } = AppRoles.Member;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        DateOfBirthValidation.Validate(DateOfBirth, nameof(DateOfBirth));
}
