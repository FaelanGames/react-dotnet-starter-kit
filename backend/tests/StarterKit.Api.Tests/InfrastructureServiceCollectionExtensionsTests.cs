using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Domain.Interfaces.Repositories;
using StarterKit.Domain.Interfaces.Security;
using StarterKit.Domain.Interfaces.Services;
using StarterKit.Domain.Interfaces.System;
using StarterKit.Infrastructure.Data;
using StarterKit.Infrastructure.Extensions;
using StarterKit.Infrastructure.Repositories;
using StarterKit.Infrastructure.Security;

namespace StarterKit.Api.Tests;

public sealed class InfrastructureServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStarterKitInfrastructure_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Data Source=:memory:",
                ["Jwt:Issuer"] = "StarterKit",
                ["Jwt:Audience"] = "StarterKit",
                ["Jwt:SigningKey"] = "this-is-a-long-signing-key-at-least-32-characters",
                ["Jwt:ExpiresMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "14",
            })
            .Build();

        services.AddStarterKitInfrastructure(config);

        Assert.Contains(services, d => d.ServiceType == typeof(IUserRepository) && d.ImplementationType == typeof(UserRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(IRefreshTokenRepository) && d.ImplementationType == typeof(RefreshTokenRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(IUnitOfWork) && d.ImplementationType == typeof(UnitOfWork));
        Assert.Contains(services, d => d.ServiceType == typeof(ITokenService) && d.ImplementationType == typeof(JwtTokenService));
        Assert.Contains(services, d => d.ServiceType == typeof(ITokenHashingService) && d.ImplementationType == typeof(JwtTokenService));
        Assert.Contains(services, d => d.ServiceType == typeof(ISystemClock) && d.ImplementationType == typeof(SystemClock));
        Assert.Contains(services, d => d.ServiceType == typeof(IPasswordHasher) && d.ImplementationType == typeof(PasswordHasher));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        Assert.NotNull(scopedProvider.GetService<AppDbContext>());
        Assert.NotNull(scopedProvider.GetService<IUserRepository>());
        Assert.NotNull(scopedProvider.GetService<IRefreshTokenRepository>());
        Assert.NotNull(scopedProvider.GetService<IUnitOfWork>());
        Assert.NotNull(provider.GetService<ITokenService>());
        Assert.NotNull(provider.GetService<ITokenHashingService>());
        Assert.NotNull(provider.GetService<ISystemClock>());
        Assert.NotNull(provider.GetService<IPasswordHasher>());
    }
}
