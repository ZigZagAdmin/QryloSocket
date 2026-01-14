using Microsoft.EntityFrameworkCore;
using QryloSocketAPI.Entities;

namespace QryloSocketAPI.Database;

public class QryloContext : DbContext
{
    public QryloContext(DbContextOptions<QryloContext> options) : base(options)
    {
    }

    public DbSet<Conversations> Conversations { get; set; }
    public DbSet<ConversationMembers> ConversationMembers { get; set; }
    public DbSet<Messages> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QryloContext).Assembly);
    }
}