using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services.Implementations;

internal sealed class UserService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserRepository userRepository,
    IUserIdentityRepository userIdentityRepository,
    ICurrentUserService currentUser,
    AuditStamper auditStamper,
    IMapper mapper) : IUserService
{
    public async Task<List<UserViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllWithIdentityAsync(cancellationToken);
        var roles = await userRepository.GetRoleNamesByUserAsync(cancellationToken);
        var messageCounts = await userRepository.GetChatMessageCountsAsync(cancellationToken);

        var models = mapper.Map<List<UserViewModel>>(users);
        foreach (var model in models)
        {
            model.Role = PrimaryRole(roles.GetValueOrDefault(model.Id));
            model.MessageCount = messageCounts.GetValueOrDefault(model.Id);
        }

        return models;
    }

    public async Task<UserViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetWithIdentityAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var model = mapper.Map<UserViewModel>(user);
        model.Role = PrimaryRole(await userManager.GetRolesAsync(user));
        model.MessageCount = await userRepository.CountChatMessagesAsync(id, cancellationToken);
        return model;
    }

    public async Task<UserUpdateModel?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetWithIdentityAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var model = mapper.Map<UserUpdateModel>(user);
        model.Role = PrimaryRole(await userManager.GetRolesAsync(user));
        return model;
    }

    public async Task<ServiceResult> CreateAsync(UserCreateModel model, CancellationToken cancellationToken = default)
    {
        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            UserIdentity = auditStamper.StampCreated(mapper.Map<UserIdentity>(model)),
        };

        var created = await userManager.CreateAsync(user, model.Password);
        if (!created.Succeeded)
        {
            return ServiceResult.Invalid(string.Join(" ", created.ErrorMessages()));
        }

        var roleAdded = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleAdded.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return ServiceResult.Invalid(string.Join(" ", roleAdded.ErrorMessages()));
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(UserUpdateModel model, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(model.Id.ToString());
        if (user is null)
        {
            return ServiceResult.NotFound();
        }

        var isSelf = user.Id == currentUser.UserId;
        var currentRole = PrimaryRole(await userManager.GetRolesAsync(user));
        if (isSelf && model.Role != currentRole)
        {
            return ServiceResult.Invalid("You can't change your own role. Ask another admin to do it.");
        }

        var email = model.Email.Trim();
        var emailChanged = !string.Equals(user.Email, email, StringComparison.Ordinal);
        if (emailChanged)
        {
            var owner = await userManager.FindByEmailAsync(email);
            if (owner is not null && owner.Id != user.Id)
            {
                return ServiceResult.Invalid($"Email '{email}' is already taken.");
            }
        }

        var userIdentity = await userIdentityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (userIdentity is null)
        {
            userIdentity = auditStamper.StampCreated(mapper.Map<UserIdentity>(model));
            userIdentity.UserId = user.Id;
            await userIdentityRepository.AddAsync(userIdentity, cancellationToken);
        }
        else
        {
            mapper.Map(model, userIdentity);
            auditStamper.StampModified(userIdentity);
        }

        await userIdentityRepository.SaveChangesAsync(cancellationToken);

        if (emailChanged)
        {
            var emailSet = await userManager.SetEmailAsync(user, email);
            if (!emailSet.Succeeded)
            {
                return ServiceResult.Invalid(string.Join(" ", emailSet.ErrorMessages()));
            }

            var userNameSet = await userManager.SetUserNameAsync(user, email);
            if (!userNameSet.Succeeded)
            {
                return ServiceResult.Invalid(string.Join(" ", userNameSet.ErrorMessages()));
            }
        }

        if (model.Role != currentRole)
        {
            if (currentRole.Length > 0)
            {
                await userManager.RemoveFromRoleAsync(user, currentRole);
            }

            await userManager.AddToRoleAsync(user, model.Role);
            await userManager.UpdateSecurityStampAsync(user);
        }

        if (isSelf)
        {
            await signInManager.RefreshSignInAsync(user);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return ServiceResult.NotFound();
        }

        if (user.Id == currentUser.UserId)
        {
            return ServiceResult.Invalid("You can't delete your own account.");
        }

        var deleted = await userManager.DeleteAsync(user);
        return deleted.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Invalid(string.Join(" ", deleted.ErrorMessages()));
    }

    public async Task<ServiceResult> SetLockedAsync(Guid id, bool locked, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return ServiceResult.NotFound();
        }

        if (user.Id == currentUser.UserId)
        {
            return ServiceResult.Invalid("You can't lock or unlock your own account.");
        }

        if (locked)
        {
            var enabled = await userManager.SetLockoutEnabledAsync(user, true);
            var lockedOut = enabled.Succeeded
                ? await userManager.SetLockoutEndDateAsync(user, AccountLockout.LockedByAdminUntil)
                : enabled;
            if (!lockedOut.Succeeded)
            {
                return ServiceResult.Invalid(string.Join(" ", lockedOut.ErrorMessages()));
            }

            await userManager.UpdateSecurityStampAsync(user);
            return ServiceResult.Success();
        }

        var unlocked = await userManager.SetLockoutEndDateAsync(user, null);
        if (!unlocked.Succeeded)
        {
            return ServiceResult.Invalid(string.Join(" ", unlocked.ErrorMessages()));
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return ServiceResult.Success();
    }

    private static string PrimaryRole(IEnumerable<string>? roles)
    {
        var list = roles?.ToList() ?? [];
        if (list.Contains(AppRoles.Admin))
        {
            return AppRoles.Admin;
        }

        return list.Contains(AppRoles.Member) ? AppRoles.Member : string.Empty;
    }
}
