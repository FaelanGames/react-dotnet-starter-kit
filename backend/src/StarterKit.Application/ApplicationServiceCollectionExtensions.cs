using Microsoft.Extensions.DependencyInjection;
using StarterKit.Application.Interfaces;
using StarterKit.Application.Services;

namespace StarterKit.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddStarterKitApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
