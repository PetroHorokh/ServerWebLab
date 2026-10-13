using BLL.Common;
using BLL.Models.Actions;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class UsersController(IUserService userService, ICurrentUserService currentUser) : ModalControllerBase
{
    private const string FormView = "_UserForm";

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await userService.GetAllAsync(cancellationToken));
    }

    [HttpGet]
    public IActionResult Create()
    {
        return Form(new UserCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Input")] UserCreateModel input, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await userService.CreateAsync(input, cancellationToken);
            if (result.Succeeded)
            {
                return ModalSuccess($"User \"{input.Email.Trim()}\" was added.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return Form(input);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var input = await userService.GetForUpdateAsync(id, cancellationToken);
        return input is null ? NotFound() : Form(input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind(Prefix = "Input")] UserUpdateModel input, CancellationToken cancellationToken)
    {
        input.Id = id;
        if (ModelState.IsValid)
        {
            var result = await userService.UpdateAsync(input, cancellationToken);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Succeeded)
            {
                return ModalSuccess($"User \"{input.Email.Trim()}\" was updated.");
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        return Form(input);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == currentUser.UserId)
        {
            return BadRequest("You can't delete your own account.");
        }

        var messages = user.MessageCount == 0
            ? string.Empty
            : $" {Pluralize(user.MessageCount, "chat message", "chat messages")} written by this user will be deleted too.";

        return PartialView(DeleteConfirmationView, new DeleteConfirmationViewModel
        {
            Title = "Delete User",
            ItemName = $"{user.DisplayName} ({user.Email})",
            Details = "Their account, profile and role are removed." + messages,
            ActionUrl = Url.Action(nameof(Delete), new { id })!,
        });
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.DeleteAsync(id, cancellationToken);
        return result.Status switch
        {
            ServiceResultStatus.Success => ModalSuccess("The user was deleted."),
            ServiceResultStatus.NotFound => NotFound(),
            _ => BadRequest(result.Error),
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Lock(Guid id, CancellationToken cancellationToken) =>
        SetLockedAsync(id, locked: true, cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken) =>
        SetLockedAsync(id, locked: false, cancellationToken);

    private async Task<IActionResult> SetLockedAsync(Guid id, bool locked, CancellationToken cancellationToken)
    {
        var result = await userService.SetLockedAsync(id, locked, cancellationToken);
        if (result.Status == ServiceResultStatus.NotFound)
        {
            return NotFound();
        }

        if (result.Succeeded)
        {
            SetStatusMessage(locked
                ? "The user was locked and can no longer log in."
                : "The user was unlocked.");
        }
        else
        {
            SetStatusMessage(result.Error!, isError: true);
        }

        return RedirectToAction(nameof(Index));
    }

    private PartialViewResult Form(UserModelBase input)
    {
        return ModalForm(FormView, new UserFormViewModel
        {
            Input = input,
            RoleOptions = AppRoles.All.Select(role => new SelectListItem(role, role)).ToList(),
            IsSelf = input is UserUpdateModel update && update.Id == currentUser.UserId,
        });
    }
}
