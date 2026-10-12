using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IActorRepository : IRepository<Actor>
{
    Task<List<Actor>> GetAllWithMoviesAsync(CancellationToken cancellationToken = default);

    Task<Actor?> GetByIdWithMoviesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Actor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}
