using BLL.Common;
using BLL.Models.Actions;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class MovieActorsController(
    IMovieActorService movieActorService,
    IMovieService movieService,
    IActorService actorService) : ModalControllerBase
{
    private const string FormView = "_MovieActorForm";

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await movieActorService.GetAllAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return await FormAsync(new MovieActorCreateModel(), cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Input")] MovieActorCreateModel input, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await movieActorService.CreateAsync(input, cancellationToken);
            if (result.Succeeded)
            {
                return ModalSuccess("The role was added.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return await FormAsync(input, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid movieId, Guid actorId, CancellationToken cancellationToken)
    {
        var input = await movieActorService.GetForUpdateAsync(movieId, actorId, cancellationToken);
        return input is null ? NotFound() : await FormAsync(input, cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid movieId,
        Guid actorId,
        [Bind(Prefix = "Input")] MovieActorUpdateModel input,
        CancellationToken cancellationToken)
    {
        input.MovieId = movieId;
        input.ActorId = actorId;
        if (ModelState.IsValid)
        {
            var result = await movieActorService.UpdateAsync(input, cancellationToken);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Succeeded)
            {
                return ModalSuccess("The role was updated.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return await FormAsync(input, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid movieId, Guid actorId, CancellationToken cancellationToken)
    {
        var role = await movieActorService.GetAsync(movieId, actorId, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        return PartialView(DeleteConfirmationView, new DeleteConfirmationViewModel
        {
            Title = "Delete Role",
            ItemName = $"{role.ActorFullName} in {role.MovieDisplayName}",
            Details = "The movie and the actor themselves are kept.",
            ActionUrl = Url.Action(nameof(Delete), new { movieId, actorId })!,
        });
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid movieId, Guid actorId, CancellationToken cancellationToken)
    {
        var result = await movieActorService.DeleteAsync(movieId, actorId, cancellationToken);
        return result.Succeeded ? ModalSuccess("The role was deleted.") : NotFound();
    }

    private async Task<IActionResult> FormAsync(MovieActorCreateModel input, CancellationToken cancellationToken)
    {
        var movies = await movieService.GetSummariesAsync(cancellationToken);
        var actors = await actorService.GetSummariesAsync(cancellationToken);

        return ModalForm(FormView, new MovieActorFormViewModel
        {
            Input = input,
            MovieOptions = movies.Select(m => new SelectListItem(m.DisplayName, m.Id.ToString())).ToList(),
            ActorOptions = actors.Select(a => new SelectListItem(a.FullName, a.Id.ToString())).ToList(),
        });
    }
}
