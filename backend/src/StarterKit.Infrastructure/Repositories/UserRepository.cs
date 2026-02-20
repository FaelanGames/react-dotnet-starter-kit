using Microsoft.EntityFrameworkCore;
using StarterKit.Domain.Entities;
using StarterKit.Domain.Interfaces.Repositories;
using StarterKit.Infrastructure.Data;

namespace StarterKit.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => _db.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => _db.Users.AddAsync(user, cancellationToken).AsTask();
}
