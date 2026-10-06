using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;

namespace BLL.Services.Interfaces;

public interface IMovieService
{
    Task<List<MovieViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<MovieSummaryViewModel>> GetSummariesAsync(CancellationToken cancellationToken = default);

    Task<MovieViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MovieDetailsViewModel?> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MovieUpdateModel?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResult> CreateAsync(MovieCreateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(MovieUpdateModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
