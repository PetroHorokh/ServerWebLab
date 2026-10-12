using BLL.Common;

namespace BLL.Models.ViewModels;

public class ChatMessageViewModel
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public Guid AuthorId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string RoleTag { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime PostedDateTime { get; set; }

    public bool IsAdmin => RoleTag == AppRoles.Admin;
}
