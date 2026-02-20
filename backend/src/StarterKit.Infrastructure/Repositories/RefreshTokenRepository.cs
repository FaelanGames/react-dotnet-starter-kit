using Microsoft.EntityFrameworkCore;
using StarterKit.Domain.Entities;
using StarterKit.Domain.Interfaces.Repositories;
using StarterKit.Infrastructure.Data;

namespace StarterKit.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        => _db.RefreshTokens.AddAsync(token, cancellationToken).AsTask();

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _db.RefreshTokens.SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
