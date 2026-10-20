using System.ComponentModel.DataAnnotations;
using BLL.Common;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class ChatAttachmentViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(ChatAttachmentConstraints.FileNameMaxLength)]
    [Display(Name = "File name")]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatAttachmentConstraints.ContentTypeMaxLength)]
    [Display(Name = "Type")]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, ChatAttachmentConstraints.MaxFileSizeBytes)]
    [Display(Name = "Size")]
    public int SizeBytes { get; set; }

    public bool IsImage => ChatAttachmentRules.IsImageContentType(ContentType);

    public string SizeText => FileSizeFormatter.Format(SizeBytes);
}
