using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CinemaDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IActorRepository, ActorRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IMovieActorRepository, MovieActorRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IUserIdentityRepository, UserIdentityRepository>();

        return services;
    }
}
