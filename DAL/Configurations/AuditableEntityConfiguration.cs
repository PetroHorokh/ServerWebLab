using DAL.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

internal static class AuditableEntityConfiguration
{
    public static void ConfigureAuditColumns<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedDateTime).IsRequired();
        builder.Property(e => e.ModifiedBy);
        builder.Property(e => e.ModifiedDateTime);
    }
}
