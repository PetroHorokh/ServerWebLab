using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.Actions;

public class LoginModel
{
    [Required]
    [EmailAddress]
    [StringLength(UserConstraints.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
