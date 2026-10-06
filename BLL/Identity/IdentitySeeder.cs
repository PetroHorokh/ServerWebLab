using AutoMapper;
using BLL.Common;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Identity;

public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(this IServiceProvider services, DefaultAdminOptions admin)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var userIdentityRepository = scope.ServiceProvider.GetRequiredService<IUserIdentityRepository>();
        var auditStamper = scope.ServiceProvider.GetRequiredService<AuditStamper>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole<Guid>(role)), $"create role '{role}'");
            }
        }

        var user = await userManager.FindByEmailAsync(admin.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = admin.Id,
                UserName = admin.Email,
                Email = admin.Email,
                EmailConfirmed = true,
                UserIdentity = auditStamper.StampCreated(mapper.Map<UserIdentity>(admin), admin.Id),
            };
            EnsureSucceeded(await userManager.CreateAsync(user, admin.Password), $"create the default admin '{admin.Email}'");
        }
        else if (await userIdentityRepository.GetByUserIdAsync(user.Id) is null)
        {
            var userIdentity = auditStamper.StampCreated(mapper.Map<UserIdentity>(admin), user.Id);
            userIdentity.UserId = user.Id;
            await userIdentityRepository.AddAsync(userIdentity);
            await userIdentityRepository.SaveChangesAsync();
        }

        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(user, AppRoles.Admin), $"add '{admin.Email}' to the {AppRoles.Admin} role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not {action}: {errors}");
        }
    }
}
