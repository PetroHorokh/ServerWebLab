using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using BLL.Services.Interfaces;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services.Implementations;

internal sealed class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    AuditStamper auditStamper,
    IMapper mapper) : IAccountService
{
    public async Task<AccountResult> RegisterAsync(RegisterModel model)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
        };
        user.UserIdentity = auditStamper.StampCreated(mapper.Map<UserIdentity>(model), user.Id);

        var created = await userManager.CreateAsync(user, model.Password);
        if (!created.Succeeded)
        {
            var duplicateEmail = created.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail));
            return AccountResult.Failed(created.Errors
                .Where(e => !(duplicateEmail && e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
                .Select(e => e.Description)
                .ToArray());
        }

        var roleAdded = await userManager.AddToRoleAsync(user, AppRoles.Member);
        if (!roleAdded.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return AccountResult.Failed(roleAdded.Errors.Select(e => e.Description).ToArray());
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return AccountResult.Success();
    }

    public async Task<AccountResult> LoginAsync(LoginModel model)
    {
        var result = await signInManager.PasswordSignInAsync(
            model.Email.Trim(), model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return AccountResult.Success();
        }

        return result.IsLockedOut
            ? AccountResult.LockedOut()
            : AccountResult.Failed("Invalid email or password.");
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
