using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class MovieActorViewModel
{
    public Guid MovieId { get; set; }

    [Required]
    [StringLength(MovieConstraints.TitleMaxLength)]
    [Display(Name = "Movie")]
    public string MovieTitle { get; set; } = string.Empty;

    [Range(MovieConstraints.MinReleaseYear, MovieConstraints.MaxReleaseYear)]
    [Display(Name = "Release year")]
    public int MovieReleaseYear { get; set; }

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

    public string MovieDisplayName => $"{MovieTitle} ({MovieReleaseYear})";

    public string ActorFullName => $"{ActorFirstName} {ActorLastName}";
}
