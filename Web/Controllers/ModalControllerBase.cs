using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public abstract class ModalControllerBase : Controller
{
    public const string StatusMessageKey = "StatusMessage";

    protected const string DeleteConfirmationView = "_DeleteConfirmation";

    protected JsonResult ModalSuccess(string message)
    {
        TempData[StatusMessageKey] = message;
        return Json(new { success = true });
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
