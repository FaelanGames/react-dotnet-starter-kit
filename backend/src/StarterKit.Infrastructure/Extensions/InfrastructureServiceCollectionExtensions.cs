using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Domain.Interfaces.Repositories;
using StarterKit.Domain.Interfaces.Security;
using StarterKit.Domain.Interfaces.Services;
using StarterKit.Domain.Interfaces.System;
using StarterKit.Infrastructure.Data;
using StarterKit.Infrastructure.Repositories;
using StarterKit.Infrastructure.Security;
using StarterKit.Infrastructure.Settings;

namespace StarterKit.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddStarterKitInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required");

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<ITokenHashingService, JwtTokenService>();
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
