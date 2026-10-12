using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace DAL.Entities;

public class UserIdentity : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [Required]
    [StringLength(UserIdentityConstraints.DisplayNameMaxLength, MinimumLength = UserIdentityConstraints.DisplayNameMinLength)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(UserIdentityConstraints.FirstNameMaxLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(UserIdentityConstraints.LastNameMaxLength)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
