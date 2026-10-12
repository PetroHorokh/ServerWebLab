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

        builder.Property(u => u.DisplayName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();

        builder.HasIndex(u => u.UserId).IsUnique();

        builder.HasOne(u => u.User)
            .WithOne(user => user.UserIdentity)
            .HasForeignKey<UserIdentity>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditColumns();
    }
}
