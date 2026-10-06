using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class MovieActorRepository(CinemaDbContext context) : Repository<MovieActor>(context), IMovieActorRepository
{
    public Task<List<MovieActor>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(ma => ma.Movie)
            .Include(ma => ma.Actor)
            .OrderBy(ma => ma.Movie.Title)
            .ThenBy(ma => ma.BillingOrder == null)
            .ThenBy(ma => ma.BillingOrder)
            .ThenBy(ma => ma.Actor.LastName)
            .ToListAsync(cancellationToken);

    public Task<MovieActor?> GetAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default) =>
        Set.Include(ma => ma.Movie)
            .Include(ma => ma.Actor)
            .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId, cancellationToken);

    public Task<bool> ExistsAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId, cancellationToken);
}
