using System.ComponentModel.DataAnnotations;
using BLL.Common;
using DAL.Constraints;

namespace BLL.Models.ViewModels;

public class ChatMessageViewModel
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public Guid AuthorId { get; set; }

    [Required]
    [StringLength(UserIdentityConstraints.DisplayNameMaxLength)]
    [Display(Name = "Author")]
    public string AuthorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatMessageConstraints.RoleTagMaxLength)]
    [Display(Name = "Role")]
    public string RoleTag { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatMessageConstraints.MessageMaxLength)]
    public string Message { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    [Display(Name = "Posted")]
    public DateTime PostedDateTime { get; set; }

    public bool IsAdmin => RoleTag == AppRoles.Admin;
}
