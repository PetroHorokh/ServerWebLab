using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;

namespace BLL.Services.Interfaces;

public interface IMovieActorService
{
    Task<List<MovieActorViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MovieActorViewModel?> GetAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default);

    Task<MovieActorUpdateModel?> GetForUpdateAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default);

    Task<ServiceResult> CreateAsync(MovieActorCreateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(MovieActorUpdateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default);
}
