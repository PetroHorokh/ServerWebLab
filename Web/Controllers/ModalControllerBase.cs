using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public abstract class ModalControllerBase : Controller
{
    public const string StatusMessageKey = "StatusMessage";

    public const string StatusMessageIsErrorKey = "StatusMessageIsError";

    protected const string DeleteConfirmationView = "_DeleteConfirmation";

    protected JsonResult ModalSuccess(string message)
    {
        SetStatusMessage(message);
        return Json(new { success = true });
    }

    protected void SetStatusMessage(string message, bool isError = false)
    {
        TempData[StatusMessageKey] = message;
        TempData[StatusMessageIsErrorKey] = isError;
    }

    protected PartialViewResult ModalForm(string viewName, object model)
    {
        var result = PartialView(viewName, model);
        if (!ModelState.IsValid)
        {
            result.StatusCode = StatusCodes.Status400BadRequest;
        }

        return result;
    }

    protected static string Pluralize(int count, string singular, string plural) =>
        $"{count} {(count == 1 ? singular : plural)}";
}
