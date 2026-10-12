using BLL.Services.Interfaces;
using DAL.Entities;

namespace BLL.Common;

internal sealed class AuditStamper(ICurrentUserService currentUser, TimeProvider timeProvider)
{
    public TEntity StampCreated<TEntity>(TEntity entity)
        where TEntity : AuditableEntity =>
        StampCreated(entity, currentUser.UserId);

    public TEntity StampCreated<TEntity>(TEntity entity, Guid createdBy)
        where TEntity : AuditableEntity
    {
        entity.CreatedBy = createdBy;
        entity.CreatedDateTime = timeProvider.GetUtcNow().UtcDateTime;
        return entity;
    }

    public TEntity StampModified<TEntity>(TEntity entity)
        where TEntity : AuditableEntity
    {
        entity.ModifiedBy = currentUser.UserId;
        entity.ModifiedDateTime = timeProvider.GetUtcNow().UtcDateTime;
        return entity;
    }
}
