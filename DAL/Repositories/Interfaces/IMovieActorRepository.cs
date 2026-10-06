using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IMovieActorRepository : IRepository<MovieActor>
{
    Task<List<MovieActor>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    Task<MovieActor?> GetAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default);
}
