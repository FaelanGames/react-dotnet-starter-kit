using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using StarterKit.Domain.Entities;
using StarterKit.Infrastructure.Data;
using StarterKit.Infrastructure.Repositories;

namespace StarterKit.Api.Tests;

public sealed class RepositoryTests
{
    [Fact]
    public async Task UserRepository_AddAndQuery_Works()
    {
        await using var db = await CreateDbContextAsync();
        var repo = new UserRepository(db);

        var user = User.CreateNew("user@example.com", "hash");
        await repo.AddAsync(user, It.IsAny<CancellationToken>());
        await db.SaveChangesAsync(It.IsAny<CancellationToken>());

        var exists = await repo.EmailExistsAsync("user@example.com", It.IsAny<CancellationToken>());
        var byEmail = await repo.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>());
        var byId = await repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>());

        Assert.True(exists);
        Assert.NotNull(byEmail);
        Assert.NotNull(byId);
        Assert.Equal(user.Id, byEmail!.Id);
        Assert.Equal(user.Id, byId!.Id);
    }

    [Fact]
    public async Task RefreshTokenRepository_AddAndGetByHash_Works()
    {
        await using var db = await CreateDbContextAsync();
        var user = User.CreateNew("token-user@example.com", "hash");
        db.Users.Add(user);
        await db.SaveChangesAsync(It.IsAny<CancellationToken>());

        var repo = new RefreshTokenRepository(db);
        var token = new RefreshToken(user.Id, "token-hash", DateTime.UtcNow.AddDays(1));

        await repo.AddAsync(token, It.IsAny<CancellationToken>());
        await repo.SaveChangesAsync(It.IsAny<CancellationToken>());

        var found = await repo.GetByHashAsync("token-hash", It.IsAny<CancellationToken>());

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.UserId);
    }

    [Fact]
    public async Task UnitOfWork_SaveChanges_PersistsPendingEntities()
    {
        await using var db = await CreateDbContextAsync();
        var unitOfWork = new UnitOfWork(db);

        db.Users.Add(User.CreateNew("uow@example.com", "hash"));
        await unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>());

        var saved = await db.Users.SingleOrDefaultAsync(u => u.Email == "uow@example.com", It.IsAny<CancellationToken>());
        Assert.NotNull(saved);
    }

    private static async Task<AppDbContext> CreateDbContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
