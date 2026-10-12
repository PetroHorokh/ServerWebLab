using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class ActorSummaryViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(ActorConstraints.FirstNameMaxLength)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(ActorConstraints.LastNameMaxLength)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
