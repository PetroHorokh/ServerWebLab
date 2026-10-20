using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

public sealed class CurrentUserHubFilter : IHubFilter
{
    public ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        invocationContext.ServiceProvider.GetRequiredService<HubCallerUser>().Principal = invocationContext.Context.User;
        return next(invocationContext);
    }
}
