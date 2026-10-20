using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<List<ChatMessage>> GetLatestForMovieAsync(Guid movieId, int count, Guid? viewerId, bool includeAllPrivate, CancellationToken cancellationToken = default);

    Task<List<ChatMessage>> GetForMovieAfterAsync(Guid movieId, Guid afterMessageId, Guid? viewerId, bool includeAllPrivate, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetDeletedIdsForMovieSinceAsync(Guid movieId, DateTime sinceUtc, CancellationToken cancellationToken = default);

    Task<List<ChatMessage>> GetLatestPrivateAsync(Guid? movieId, int count, CancellationToken cancellationToken = default);

    Task<List<(Guid MovieId, string Title, int Count)>> GetPrivateCountsByMovieAsync(CancellationToken cancellationToken = default);

    Task<ChatMessage?> GetWithAuthorAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
