using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<List<ApplicationUser>> GetAllWithIdentityAsync(CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetWithIdentityAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, List<string>>> GetRoleNamesByUserAsync(CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, int>> GetChatMessageCountsAsync(CancellationToken cancellationToken = default);

    Task<int> CountChatMessagesAsync(Guid userId, CancellationToken cancellationToken = default);
}
