using BLL.Common;
using BLL.Models.Actions;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = AppRoles.AdminOrMember)]
public class ChatController(IChatService chatService) : ModalControllerBase
{
    public const string RemovedHeader = "X-Chat-Removed";
    public const string SyncedHeader = "X-Chat-Synced";

    private const string MessagesView = "_ChatMessages";
    private const int ExcerptLength = 120;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Messages(Guid movieId, Guid? afterId, long? since, CancellationToken cancellationToken)
    {
        DateTime? sinceUtc = since is { } ticks && ticks > 0 && ticks <= DateTime.MaxValue.Ticks
            ? new DateTime(ticks, DateTimeKind.Utc)
            : null;

        var updates = await chatService.GetUpdatesAsync(movieId, afterId, sinceUtc, cancellationToken);

        Response.Headers[SyncedHeader] = updates.SyncedAt.Ticks.ToString();
        Response.Headers[RemovedHeader] = string.Join(",", updates.RemovedMessageIds);
        return PartialView(MessagesView, updates.Messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid movieId, ChatMessageCreateModel input, CancellationToken cancellationToken)
    {
        var isAjax = Request.Headers.XRequestedWith == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            var error = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
            if (isAjax)
            {
                return BadRequest(error);
            }

            TempData[StatusMessageKey] = error;
            return RedirectToAction(nameof(MoviesController.Details), "Movies", new { id = movieId });
        }

        var message = await chatService.SendAsync(movieId, input, cancellationToken);
        if (message is null)
        {
            return NotFound();
        }

        return isAjax
            ? PartialView(MessagesView, new[] { message })
            : RedirectToAction(nameof(MoviesController.Details), "Movies", new { id = movieId });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var message = await chatService.GetAsync(id, cancellationToken);
        if (message is null)
        {
            return NotFound();
        }

        var excerpt = message.Message.Length > ExcerptLength
            ? message.Message[..ExcerptLength].TrimEnd() + "…"
            : message.Message;

        return PartialView(DeleteConfirmationView, new DeleteConfirmationViewModel
        {
            Title = "Delete Message",
            ItemName = $"this message from {message.AuthorName}",
            Details = $"“{excerpt}”",
            ActionUrl = Url.Action(nameof(Delete), new { id })!,
        });
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var result = await chatService.DeleteAsync(id, cancellationToken);
        return result.Succeeded ? ModalSuccess("The message was deleted.") : NotFound();
    }
}
