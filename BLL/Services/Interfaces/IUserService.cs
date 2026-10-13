using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;

namespace BLL.Services.Interfaces;

public interface IUserService
{
    Task<List<UserViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserUpdateModel?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResult> CreateAsync(UserCreateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(UserUpdateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResult> SetLockedAsync(Guid id, bool locked, CancellationToken cancellationToken = default);
}
