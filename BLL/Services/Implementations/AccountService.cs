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
            return AccountResult.Failed(created.ErrorMessages());
        }

        var roleAdded = await userManager.AddToRoleAsync(user, AppRoles.Member);
        if (!roleAdded.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return AccountResult.Failed(roleAdded.ErrorMessages());
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

        if (!result.IsLockedOut)
        {
            return AccountResult.Failed("Invalid email or password.");
        }

        var user = await userManager.FindByEmailAsync(model.Email.Trim());
        return user is not null && AccountLockout.IsLockedByAdmin(user.LockoutEnd)
            ? AccountResult.Failed("This account has been locked by an administrator.")
            : AccountResult.LockedOut();
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
