using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace DAL.Entities;

public class ChatAttachmentData
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ChatAttachmentConstraints.MaxFileSizeBytes)]
    public byte[] Content { get; set; } = [];
}
