using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace DAL.Entities;

public class ChatAttachment : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid ChatMessageId { get; set; }

    [Required]
    [StringLength(ChatAttachmentConstraints.FileNameMaxLength)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatAttachmentConstraints.ContentTypeMaxLength)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, ChatAttachmentConstraints.MaxFileSizeBytes)]
    public int SizeBytes { get; set; }

    public ChatMessage ChatMessage { get; set; } = null!;

    public ChatAttachmentData Data { get; set; } = null!;
}
