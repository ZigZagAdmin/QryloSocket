using Microsoft.EntityFrameworkCore;
using QryloSocketAPI.Entities;

namespace QryloSocketAPI.Database;

public class QryloContext : DbContext
{
    public QryloContext(DbContextOptions<QryloContext> options) : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMember> ConversationMembers { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QryloContext).Assembly);
    }
}