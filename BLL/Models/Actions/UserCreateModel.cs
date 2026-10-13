using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.Actions;

public class UserCreateModel : UserModelBase
{
    [Required]
    [StringLength(UserConstraints.PasswordMaxLength, MinimumLength = UserConstraints.PasswordMinLength)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords don't match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
