using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class MovieRepository(CinemaDbContext context) : Repository<Movie>(context), IMovieRepository
{
    public override Task<List<Movie>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .OrderBy(m => m.Title)
            .ThenBy(m => m.ReleaseYear)
            .ToListAsync(cancellationToken);

    public Task<List<Movie>> GetAllWithActorsAsync(CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .AsSplitQuery()
            .OrderBy(m => m.Title)
            .ThenBy(m => m.ReleaseYear)
            .ToListAsync(cancellationToken);

    public Task<Movie?> GetByIdWithActorsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(m => m.Id == id, cancellationToken);

    public async Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return true;
        }

        var distinctIds = ids.Distinct().ToList();
        var found = await Set.CountAsync(m => distinctIds.Contains(m.Id), cancellationToken);
        return found == distinctIds.Count;
    }
}
