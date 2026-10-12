using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

internal sealed class UserIdentityConfiguration : IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("UserIdentity");
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.UserId).IsUnique();

        builder.HasOne(u => u.User)
            .WithOne(user => user.UserIdentity)
            .HasForeignKey<UserIdentity>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditColumns();
    }
}
