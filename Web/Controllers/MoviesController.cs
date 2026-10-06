using BLL.Common;
using BLL.Models.Actions;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = AppRoles.AdminOrMember)]
public class MoviesController(IMovieService movieService, IActorService actorService, IChatService chatService) : ModalControllerBase
{
    private const string FormView = "_MovieForm";

    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var movies = await movieService.GetAllAsync(cancellationToken);
        return User.IsInRole(AppRoles.Admin) ? View(movies) : View("Catalog", movies);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var movie = await movieService.GetDetailsAsync(id, cancellationToken);
        if (movie is null)
        {
            return NotFound();
        }

        var chat = await chatService.GetUpdatesAsync(id, afterMessageId: null, sinceUtc: null, cancellationToken);

        return View(new MovieDetailsPageViewModel
        {
            Movie = movie,
            Messages = chat.Messages,
            ChatSyncedAt = chat.SyncedAt,
        });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return await FormAsync(new MovieCreateModel(), cancellationToken);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Input")] MovieCreateModel input, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await movieService.CreateAsync(input, cancellationToken);
            if (result.Succeeded)
            {
                return ModalSuccess($"Movie \"{input.Title}\" was added.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return await FormAsync(input, cancellationToken);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var input = await movieService.GetForUpdateAsync(id, cancellationToken);
        return input is null ? NotFound() : await FormAsync(input, cancellationToken);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind(Prefix = "Input")] MovieUpdateModel input, CancellationToken cancellationToken)
    {
        input.Id = id;
        if (ModelState.IsValid)
        {
            var result = await movieService.UpdateAsync(input, cancellationToken);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Succeeded)
            {
                return ModalSuccess($"Movie \"{input.Title}\" was updated.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return await FormAsync(input, cancellationToken);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var movie = await movieService.GetByIdAsync(id, cancellationToken);
        if (movie is null)
        {
            return NotFound();
        }

        var castNote = movie.Actors.Count == 0
            ? string.Empty
            : $"The roles of {Pluralize(movie.Actors.Count, "actor", "actors")} in this movie will be removed. ";

        return PartialView(DeleteConfirmationView, new DeleteConfirmationViewModel
        {
            Title = "Delete Movie",
            ItemName = movie.DisplayName,
            Details = castNote + "The movie's chat and all of its messages will be deleted as well.",
            ActionUrl = Url.Action(nameof(Delete), new { id })!,
        });
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var result = await movieService.DeleteAsync(id, cancellationToken);
        return result.Succeeded ? ModalSuccess("The movie was deleted.") : NotFound();
    }

    private async Task<IActionResult> FormAsync(MovieCreateModel input, CancellationToken cancellationToken)
    {
        var actors = await actorService.GetSummariesAsync(cancellationToken);
        var selectedIds = input.ActorIds.ToHashSet();

        return ModalForm(FormView, new MovieFormViewModel
        {
            Input = input,
            ActorOptions = actors
                .Select(a => new SelectListItem(a.FullName, a.Id.ToString(), selectedIds.Contains(a.Id)))
                .ToList(),
        });
    }
}
