using Microsoft.AspNetCore.Identity;

namespace DAL.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public UserIdentity? UserIdentity { get; set; }

    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
