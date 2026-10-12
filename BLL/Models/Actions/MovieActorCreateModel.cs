using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Actions;

public class MovieActorCreateModel
{
    [Required(ErrorMessage = "Select a movie.")]
    [Display(Name = "Movie")]
    public Guid? MovieId { get; set; }

    [Required(ErrorMessage = "Select an actor.")]
    [Display(Name = "Actor")]
    public Guid? ActorId { get; set; }

    [StringLength(150)]
    [Display(Name = "Character name")]
    public string? CharacterName { get; set; }

    [Display(Name = "Lead role")]
    public bool IsLeadRole { get; set; }

    [Range(1, 999)]
    [Display(Name = "Billing order")]
    public int? BillingOrder { get; set; }
}
