using Microsoft.EntityFrameworkCore;

namespace StarterKit.Api.Data;

public static class DbInitializer
{
    public static async Task EnsureMigratedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
    }
}
