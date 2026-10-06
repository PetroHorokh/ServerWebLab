namespace BLL.Models.ViewModels;

public class CastMemberViewModel
{
    public Guid ActorId { get; set; }

    public string ActorFirstName { get; set; } = string.Empty;

    public string ActorLastName { get; set; } = string.Empty;

    public string? CharacterName { get; set; }

    public bool IsLeadRole { get; set; }

    public int? BillingOrder { get; set; }

    public string ActorFullName => $"{ActorFirstName} {ActorLastName}";
}
