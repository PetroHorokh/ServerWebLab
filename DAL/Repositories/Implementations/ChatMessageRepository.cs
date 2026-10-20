using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class ChatMessageRepository(CinemaDbContext context) : Repository<ChatMessage>(context), IChatMessageRepository
{
    private const int MaxNewMessages = 200;

    public async Task<List<ChatMessage>> GetLatestForMovieAsync(
        Guid movieId,
        int count,
        Guid? viewerId,
        bool includeAllPrivate,
        CancellationToken cancellationToken = default)
    {
        var latest = await WithDetails(VisibleTo(Set.AsNoTracking(), viewerId, includeAllPrivate))
            .Where(m => m.MovieId == movieId)
            .OrderByDescending(m => m.PostedDateTime)
            .ThenByDescending(m => m.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

        latest.Reverse();
        return latest;
    }

    public async Task<List<ChatMessage>> GetForMovieAfterAsync(
        Guid movieId,
        Guid afterMessageId,
        Guid? viewerId,
        bool includeAllPrivate,
        CancellationToken cancellationToken = default)
    {
        var after = await Set
            .Where(m => m.Id == afterMessageId && m.MovieId == movieId)
            .Select(m => (DateTime?)m.PostedDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (after is null)
        {
            return [];
        }

        return await WithDetails(VisibleTo(Set.AsNoTracking(), viewerId, includeAllPrivate))
            .Where(m => m.MovieId == movieId && m.PostedDateTime > after.Value)
            .OrderBy(m => m.PostedDateTime)
            .ThenBy(m => m.Id)
            .Take(MaxNewMessages)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Guid>> GetDeletedIdsForMovieSinceAsync(Guid movieId, DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        Set.Where(m => m.MovieId == movieId && m.IsDeleted && m.ModifiedDateTime > sinceUtc)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

    public Task<List<ChatMessage>> GetLatestPrivateAsync(Guid? movieId, int count, CancellationToken cancellationToken = default) =>
        WithDetails(Set.AsNoTracking())
            .Include(m => m.Movie)
            .Where(m => m.IsPrivate && !m.IsDeleted && (movieId == null || m.MovieId == movieId))
            .OrderByDescending(m => m.PostedDateTime)
            .ThenByDescending(m => m.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<List<(Guid MovieId, string Title, int Count)>> GetPrivateCountsByMovieAsync(CancellationToken cancellationToken = default)
    {
        var counts = await Set
            .Where(m => m.IsPrivate && !m.IsDeleted)
            .GroupBy(m => new { m.MovieId, m.Movie.Title })
            .Select(g => new { g.Key.MovieId, g.Key.Title, Count = g.Count() })
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

        return counts.Select(x => (x.MovieId, x.Title, x.Count)).ToList();
    }

    public Task<ChatMessage?> GetWithAuthorAsync(Guid id, CancellationToken cancellationToken = default) =>
        WithDetails(Set.AsNoTracking())
            .Include(m => m.Movie)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    private static IQueryable<ChatMessage> VisibleTo(IQueryable<ChatMessage> query, Guid? viewerId, bool includeAllPrivate) =>
        query.Where(m => !m.IsDeleted && (!m.IsPrivate || includeAllPrivate || m.UserId == viewerId));

    private static IQueryable<ChatMessage> WithDetails(IQueryable<ChatMessage> query) =>
        query
            .Include(m => m.User)
                .ThenInclude(u => u.UserIdentity)
            .Include(m => m.Attachments.OrderBy(a => a.Id));
}
