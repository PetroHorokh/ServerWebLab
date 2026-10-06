using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    Task<List<Movie>> GetAllWithActorsAsync(CancellationToken cancellationToken = default);

    Task<Movie?> GetByIdWithActorsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}
