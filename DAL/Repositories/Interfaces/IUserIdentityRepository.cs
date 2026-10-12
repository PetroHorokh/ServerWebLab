using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IUserIdentityRepository : IRepository<UserIdentity>
{
    Task<UserIdentity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
