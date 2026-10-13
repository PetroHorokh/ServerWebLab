using Microsoft.AspNetCore.Identity;

namespace BLL.Common;

internal static class IdentityResultExtensions
{
    public static string[] ErrorMessages(this IdentityResult result)
    {
        var duplicateEmail = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail));
        return result.Errors
            .Where(e => !(duplicateEmail && e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
            .Select(e => e.Description)
            .ToArray();
    }
}
