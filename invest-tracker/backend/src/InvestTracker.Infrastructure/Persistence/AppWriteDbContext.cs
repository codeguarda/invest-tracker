using Microsoft.EntityFrameworkCore;
using InvestTracker.Domain.Investments;
using InvestTracker.Domain.Users;
using InvestTracker.Infrastructure.Outbox;

namespace InvestTracker.Infrastructure.Persistence;

public sealed class AppWriteDbContext : DbContext
{
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public AppWriteDbContext(DbContextOptions<AppWriteDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Investment>(cfg =>
        {
            cfg.ToTable("investments");
            cfg.HasKey(x => x.Id);
            cfg.Property(x => x.Type).HasMaxLength(80).IsRequired();
            cfg.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            cfg.Property(x => x.Date).HasColumnType("date");
            cfg.HasIndex(x => new { x.UserId, x.Date });
        });

        b.Entity<User>(cfg =>
        {
            cfg.ToTable("users");
            cfg.HasKey(x => x.Id);
            cfg.Property(x => x.Email).HasMaxLength(200).IsRequired();
            cfg.Property(x => x.PasswordHash).IsRequired();
            cfg.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<OutboxMessage>(cfg =>
        {
            cfg.ToTable("outbox_messages");
            cfg.HasKey(x => x.Id);
            cfg.HasIndex(x => x.ProcessedAtUtc);
        });
    }
}
