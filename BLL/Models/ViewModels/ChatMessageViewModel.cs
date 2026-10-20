using System.ComponentModel.DataAnnotations;
using BLL.Common;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class ChatMessageViewModel
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    [StringLength(MovieConstraints.TitleMaxLength)]
    [Display(Name = "Movie")]
    public string MovieTitle { get; set; } = string.Empty;

    public Guid AuthorId { get; set; }

    [Required]
    [StringLength(UserIdentityConstraints.DisplayNameMaxLength)]
    [Display(Name = "Author")]
    public string AuthorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatMessageConstraints.RoleTagMaxLength)]
    [Display(Name = "Role")]
    public string RoleTag { get; set; } = string.Empty;

    [StringLength(ChatMessageConstraints.MessageMaxLength)]
    public string Message { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    [Display(Name = "Posted")]
    public DateTime PostedDateTime { get; set; }

    [Display(Name = "Private")]
    public bool IsPrivate { get; set; }

    public List<ChatAttachmentViewModel> Attachments { get; set; } = [];

    public bool IsAdmin => RoleTag == AppRoles.Admin;
}
