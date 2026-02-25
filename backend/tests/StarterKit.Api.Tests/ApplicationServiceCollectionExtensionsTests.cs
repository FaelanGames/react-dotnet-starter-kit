using Microsoft.Extensions.DependencyInjection;
using StarterKit.Application;
using StarterKit.Application.Interfaces;
using StarterKit.Application.Services;

namespace StarterKit.Api.Tests;

public sealed class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStarterKitApplication_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddStarterKitApplication();

        Assert.Contains(services, d =>
            d.ServiceType == typeof(IAuthService) &&
            d.ImplementationType == typeof(AuthService) &&
            d.Lifetime == ServiceLifetime.Scoped);

        Assert.Contains(services, d =>
            d.ServiceType == typeof(IUserService) &&
            d.ImplementationType == typeof(UserService) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }
}
