using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class UserRepository(CinemaDbContext context) : Repository<ApplicationUser>(context), IUserRepository
{
    public Task<List<ApplicationUser>> GetAllWithIdentityAsync(CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(u => u.UserIdentity)
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

    public Task<ApplicationUser?> GetWithIdentityAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(u => u.UserIdentity)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<Dictionary<Guid, List<string>>> GetRoleNamesByUserAsync(CancellationToken cancellationToken = default)
    {
        var pairs = await Context.UserRoles
            .Join(Context.Roles, userRole => userRole.RoleId, role => role.Id, (userRole, role) => new { userRole.UserId, role.Name })
            .ToListAsync(cancellationToken);

        return pairs
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Name ?? string.Empty).ToList());
    }

    public Task<Dictionary<Guid, int>> GetChatMessageCountsAsync(CancellationToken cancellationToken = default) =>
        Context.ChatMessages
            .Where(m => !m.IsDeleted)
            .GroupBy(m => m.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

    public Task<int> CountChatMessagesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Context.ChatMessages.CountAsync(m => m.UserId == userId && !m.IsDeleted, cancellationToken);
}
