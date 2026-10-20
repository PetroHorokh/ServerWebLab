using System.Security.Claims;
using BLL.Services.Interfaces;
using Web.Hubs;

namespace Web.Services.Implementations;

public sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor, HubCallerUser hubCallerUser) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => hubCallerUser.Principal ?? httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new InvalidOperationException("There is no signed-in user.");
        }
    }

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
