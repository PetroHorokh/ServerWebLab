using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using DAL.Constraints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = AppRoles.AdminOrMember)]
public class ChatController(IChatService chatService) : ModalControllerBase
{
    public const long MaxSendRequestBytes =
        (long)ChatAttachmentConstraints.MaxFileSizeBytes * ChatAttachmentConstraints.MaxFilesPerMessage + 1024 * 1024;

    private const int ExcerptLength = 120;

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Private(Guid? movieId, CancellationToken cancellationToken)
    {
        var model = await chatService.GetPrivateMessagesAsync(movieId, cancellationToken);
        return model is null ? Forbid() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxSendRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxSendRequestBytes)]
    public async Task<IActionResult> Send(Guid movieId, ChatMessageCreateModel input, List<IFormFile> files, CancellationToken cancellationToken)
    {
        var result = ModelState.IsValid
            ? await chatService.SendAsync(movieId, input, files.Select(ToUpload).ToList(), cancellationToken)
            : ServiceResult<ChatMessageViewModel>.Invalid(ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage);

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return result.Status switch
            {
                ServiceResultStatus.Success => Json(result.Value),
                ServiceResultStatus.NotFound => NotFound(),
                ServiceResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error),
                _ => BadRequest(result.Error),
            };
        }

        if (result.Status == ServiceResultStatus.NotFound)
        {
            return NotFound();
        }

        if (!result.Succeeded)
        {
            SetStatusMessage(result.Error!, isError: true);
        }

        return RedirectToAction(nameof(MoviesController.Details), "Movies", new { id = movieId });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Attachment(Guid id, CancellationToken cancellationToken)
    {
        var file = await chatService.GetAttachmentAsync(id, cancellationToken);
        if (file is null)
        {
            return NotFound();
        }

        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "default-src 'none'; sandbox";
        Response.Headers.CacheControl = "private, max-age=3600";

        if (!file.IsImage)
        {
            return File(file.Content, file.ContentType, file.FileName);
        }

        var disposition = new ContentDispositionHeaderValue("inline");
        disposition.SetHttpFileName(file.FileName);
        Response.Headers.ContentDisposition = disposition.ToString();
        return File(file.Content, file.ContentType);
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

        var details = new List<string>();
        if (message.Message.Length > 0)
        {
            details.Add(message.Message.Length > ExcerptLength
                ? $"“{message.Message[..ExcerptLength].TrimEnd()}…”"
                : $"“{message.Message}”");
        }

        if (message.Attachments.Count > 0)
        {
            details.Add($"Its {Pluralize(message.Attachments.Count, "attached file", "attached files")} will be removed as well.");
        }

        return PartialView(DeleteConfirmationView, new DeleteConfirmationViewModel
        {
            Title = message.IsPrivate ? "Delete Private Message" : "Delete Message",
            ItemName = $"this message from {message.AuthorName}",
            Details = details.Count > 0 ? string.Join(" ", details) : null,
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
        return result.Status switch
        {
            ServiceResultStatus.Success => ModalSuccess("The message was deleted."),
            ServiceResultStatus.Forbidden => Forbid(),
            _ => NotFound(),
        };
    }

    private static ChatAttachmentUploadModel ToUpload(IFormFile file) => new()
    {
        FileName = file.FileName,
        Length = file.Length,
        OpenReadStream = file.OpenReadStream,
    };
}
