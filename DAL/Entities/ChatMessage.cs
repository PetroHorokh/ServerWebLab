using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace DAL.Entities;

public class ChatMessage : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public Guid UserId { get; set; }

    [Required]
    [StringLength(ChatMessageConstraints.MessageMaxLength)]
    public string Message { get; set; } = string.Empty;

    public DateTime PostedDateTime { get; set; }

    [Required]
    [StringLength(ChatMessageConstraints.RoleTagMaxLength)]
    public string RoleTag { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public Movie Movie { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}
