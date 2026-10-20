using System.Security.Claims;

namespace Web.Hubs;

public sealed class HubCallerUser
{
    public ClaimsPrincipal? Principal { get; set; }
}
