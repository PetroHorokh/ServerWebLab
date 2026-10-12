using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class ActorRepository(CinemaDbContext context) : Repository<Actor>(context), IActorRepository
{
    public override Task<List<Actor>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .ToListAsync(cancellationToken);

    public Task<List<Actor>> GetAllWithMoviesAsync(CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
            .AsSplitQuery()
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Actor?> GetByIdWithMoviesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Actor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(a => a.Id == id, cancellationToken);

    public async Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return true;
        }

        var distinctIds = ids.Distinct().ToList();
        var found = await Set.CountAsync(a => distinctIds.Contains(a.Id), cancellationToken);
        return found == distinctIds.Count;
    }
}
