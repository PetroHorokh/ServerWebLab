using System.Security.Claims;
using BLL.Common;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BLL.Identity;

internal sealed class CinemaUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOptions<IdentityOptions> options,
    IUserIdentityRepository userIdentityRepository)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var userIdentity = await userIdentityRepository.GetByUserIdAsync(user.Id);
        identity.AddClaim(new Claim(AppClaimTypes.DisplayName, userIdentity?.DisplayName ?? user.UserName ?? string.Empty));
        return identity;
    }
}
