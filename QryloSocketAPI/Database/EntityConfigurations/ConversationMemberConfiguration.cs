using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QryloSocketAPI.Entities;

namespace QryloSocketAPI.Database.EntityConfigurations;

public class ConversationMemberConfiguration: IEntityTypeConfiguration<ConversationMember>
{
    public void Configure(EntityTypeBuilder<ConversationMember> builder)
    {
        builder.Property(e => e.ConversationId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
        builder.Property(e => e.ConversationCreatedOn).HasDefaultValue(0);
        builder.Property(e => e.UserId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
        builder.Property(e => e.UserCreatedOn).HasDefaultValue(0);
        builder.Property(e => e.Permission).HasDefaultValue(0);
        builder.Property(e => e.IsBlocked).HasDefaultValue(0);
        builder.Property(e => e.CreatedOn).HasDefaultValue(0);
        builder.Property(e => e.CreatedBy).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
        builder.Property(e => e.CreatedByCreatedOn).HasDefaultValue(0);
    }
}