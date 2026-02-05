using Microsoft.EntityFrameworkCore;
using StarterKit.Api.Features.Users;

namespace StarterKit.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.CreatedUtc).IsRequired();
        });
    }
}
