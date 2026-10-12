using System.ComponentModel.DataAnnotations;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class CinemaDbContext(DbContextOptions<CinemaDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Actor> Actors => Set<Actor>();

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<MovieActor> MovieActors => Set<MovieActor>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateChangedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateChangedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CinemaDbContext).Assembly);
    }

    private void ValidateChangedEntities()
    {
        var changed = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var entity in changed)
        {
            Validator.ValidateObject(entity, new ValidationContext(entity), validateAllProperties: true);
        }
    }
}
