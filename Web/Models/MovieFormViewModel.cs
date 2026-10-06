using BLL.Models.Actions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Models;

public class MovieFormViewModel
{
    public required MovieCreateModel Input { get; init; }

    public required IReadOnlyList<SelectListItem> ActorOptions { get; init; }

    public Guid? EditId => (Input as MovieUpdateModel)?.Id;
}
