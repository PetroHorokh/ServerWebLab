using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class UserIdentityRepository(CinemaDbContext context) : Repository<UserIdentity>(context), IUserIdentityRepository
{
    public Task<UserIdentity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
}
