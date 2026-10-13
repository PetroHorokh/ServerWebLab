using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class UserViewModel
{
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(UserConstraints.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

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
    public string Role { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    [Display(Name = "Registered")]
    public DateTime? RegisteredDateTime { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    [Display(Name = "Messages")]
    public int MessageCount { get; set; }

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow;

    public string FullName => $"{FirstName} {LastName}";
}
