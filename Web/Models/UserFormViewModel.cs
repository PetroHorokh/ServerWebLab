using BLL.Models.Actions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Models;

public class UserFormViewModel
{
    public required UserModelBase Input { get; init; }

    public required IReadOnlyList<SelectListItem> RoleOptions { get; init; }

    public bool IsSelf { get; init; }

    public Guid? EditId => (Input as UserUpdateModel)?.Id;
}
