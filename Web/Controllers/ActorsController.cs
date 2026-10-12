using BLL.Common;
using BLL.Models.Actions;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class ActorsController(IActorService actorService, IMovieService movieService) : ModalControllerBase
{
    private const string FormView = "_ActorForm";

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await actorService.GetAllAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return await FormAsync(new ActorCreateModel(), cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Input")] ActorCreateModel input, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await actorService.CreateAsync(input, cancellationToken);
            if (result.Succeeded)
            {
                return ModalSuccess($"Actor \"{input.FirstName} {input.LastName}\" was added.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return await FormAsync(input, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var input = await actorService.GetForUpdateAsync(id, cancellationToken);
        return input is null ? NotFound() : await FormAsync(input, cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind(Prefix = "Input")] ActorUpdateModel input, CancellationToken cancellationToken)
    {
        input.Id = id;
        if (ModelState.IsValid)
        {
            var result = await actorService.UpdateAsync(input, cancellationToken);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Succeeded)
            {
                return ModalSuccess($"Actor \"{input.FirstName} {input.LastName}\" was updated.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return await FormAsync(input, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var actor = await actorService.GetByIdAsync(id, cancellationToken);
        if (actor is null)
        {
            return NotFound();
        }

        return PartialView(DeleteConfirmationView, new DeleteConfirmationViewModel
        {
            Title = "Delete Actor",
            ItemName = actor.FullName,
            Details = actor.Movies.Count == 0
                ? null
                : $"Their roles in {Pluralize(actor.Movies.Count, "movie", "movies")} will be removed as well.",
            ActionUrl = Url.Action(nameof(Delete), new { id })!,
        });
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var result = await actorService.DeleteAsync(id, cancellationToken);
        return result.Succeeded ? ModalSuccess("The actor was deleted.") : NotFound();
    }

    private async Task<IActionResult> FormAsync(ActorCreateModel input, CancellationToken cancellationToken)
    {
        var movies = await movieService.GetSummariesAsync(cancellationToken);
        var selectedIds = input.MovieIds.ToHashSet();

        return ModalForm(FormView, new ActorFormViewModel
        {
            Input = input,
            MovieOptions = movies
                .Select(m => new SelectListItem(m.DisplayName, m.Id.ToString(), selectedIds.Contains(m.Id)))
                .ToList(),
        });
    }
}
