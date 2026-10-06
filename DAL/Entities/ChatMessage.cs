namespace DAL.Entities;

public class ChatMessage : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public Guid UserId { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime PostedDateTime { get; set; }

    public string RoleTag { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public Movie Movie { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}
