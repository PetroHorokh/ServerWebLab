using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;

namespace BLL.Services.Interfaces;

public interface IActorService
{
    Task<List<ActorViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<ActorSummaryViewModel>> GetSummariesAsync(CancellationToken cancellationToken = default);

    Task<ActorViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActorUpdateModel?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResult> CreateAsync(ActorCreateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(ActorUpdateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
