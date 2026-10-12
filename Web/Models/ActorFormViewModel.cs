using BLL.Models.Actions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Models;

public class ActorFormViewModel
{
    public required ActorCreateModel Input { get; init; }

    public required IReadOnlyList<SelectListItem> MovieOptions { get; init; }

    public Guid? EditId => (Input as ActorUpdateModel)?.Id;
}
