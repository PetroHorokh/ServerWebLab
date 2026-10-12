using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class ChatMessageRepository(CinemaDbContext context) : Repository<ChatMessage>(context), IChatMessageRepository
{
    private const int MaxNewMessages = 200;

    public async Task<List<ChatMessage>> GetLatestForMovieAsync(Guid movieId, int count, CancellationToken cancellationToken = default)
    {
        var latest = await Set.AsNoTracking()
            .Include(m => m.User)
                .ThenInclude(u => u.UserIdentity)
            .Where(m => m.MovieId == movieId && !m.IsDeleted)
            .OrderByDescending(m => m.PostedDateTime)
            .ThenByDescending(m => m.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

        latest.Reverse();
        return latest;
    }

    public async Task<List<ChatMessage>> GetForMovieAfterAsync(Guid movieId, Guid afterMessageId, CancellationToken cancellationToken = default)
    {
        var after = await Set
            .Where(m => m.Id == afterMessageId && m.MovieId == movieId)
            .Select(m => (DateTime?)m.PostedDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (after is null)
        {
            return [];
        }

        return await Set.AsNoTracking()
            .Include(m => m.User)
                .ThenInclude(u => u.UserIdentity)
            .Where(m => m.MovieId == movieId && !m.IsDeleted && m.PostedDateTime > after.Value)
            .OrderBy(m => m.PostedDateTime)
            .ThenBy(m => m.Id)
            .Take(MaxNewMessages)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Guid>> GetDeletedIdsForMovieSinceAsync(Guid movieId, DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        Set.Where(m => m.MovieId == movieId && m.IsDeleted && m.ModifiedDateTime > sinceUtc)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

    public Task<ChatMessage?> GetWithAuthorAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(m => m.User)
                .ThenInclude(u => u.UserIdentity)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
}
