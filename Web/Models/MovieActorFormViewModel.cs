using BLL.Models.Actions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Models;

public class MovieActorFormViewModel
{
    public required MovieActorCreateModel Input { get; init; }

    public required IReadOnlyList<SelectListItem> MovieOptions { get; init; }

    public required IReadOnlyList<SelectListItem> ActorOptions { get; init; }

    public bool IsEdit => Input is MovieActorUpdateModel;
}
