namespace BLL.Models.ViewModels;

public class MovieActorViewModel
{
    public Guid MovieId { get; set; }

    public string MovieTitle { get; set; } = string.Empty;

    public int MovieReleaseYear { get; set; }

    public Guid ActorId { get; set; }

    public string ActorFirstName { get; set; } = string.Empty;

    public string ActorLastName { get; set; } = string.Empty;

    public string? CharacterName { get; set; }

    public bool IsLeadRole { get; set; }

    public int? BillingOrder { get; set; }

    public string MovieDisplayName => $"{MovieTitle} ({MovieReleaseYear})";

    public string ActorFullName => $"{ActorFirstName} {ActorLastName}";
}
