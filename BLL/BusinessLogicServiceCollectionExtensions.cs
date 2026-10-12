using BLL.Common;
using BLL.Identity;
using BLL.Mapping;
using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using DAL;
using DAL.Constraints;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BLL;

public static class BusinessLogicServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogic(
        this IServiceCollection services,
        string connectionString,
        string? autoMapperLicenseKey = null)
    {
        services.AddDataAccess(connectionString);

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = UserConstraints.PasswordMinLength;
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<CinemaDbContext>()
            .AddClaimsPrincipalFactory<CinemaUserClaimsPrincipalFactory>();

        services.AddAutoMapper(
            config =>
            {
                if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey))
                {
                    config.LicenseKey = autoMapperLicenseKey;
                }
            },
            typeof(ActorProfile).Assembly);

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditStamper>();

        services.AddScoped<IActorService, ActorService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IMovieActorService, MovieActorService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}
