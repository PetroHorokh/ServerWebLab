using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class CastMemberViewModel
{
    public Guid ActorId { get; set; }

    [Required]
    [StringLength(ActorConstraints.FirstNameMaxLength)]
    [Display(Name = "First name")]
    public string ActorFirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(ActorConstraints.LastNameMaxLength)]
    [Display(Name = "Last name")]
    public string ActorLastName { get; set; } = string.Empty;

    [StringLength(MovieActorConstraints.CharacterNameMaxLength)]
    [Display(Name = "Character name")]
    public string? CharacterName { get; set; }

    [Display(Name = "Lead role")]
    public bool IsLeadRole { get; set; }

    [Range(MovieActorConstraints.MinBillingOrder, MovieActorConstraints.MaxBillingOrder)]
    [Display(Name = "Billing order")]
    public int? BillingOrder { get; set; }

    public string ActorFullName => $"{ActorFirstName} {ActorLastName}";
}
