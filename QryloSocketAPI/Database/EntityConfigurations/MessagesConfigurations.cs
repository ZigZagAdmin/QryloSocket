using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QryloSocketAPI.Entities;

namespace QryloSocketAPI.Database.EntityConfigurations;

public class MessagesConfigurations: IEntityTypeConfiguration<Messages>
{
    public void Configure(EntityTypeBuilder<Messages> builder)
    {
        builder.Property(e => e.UserId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
        builder.Property(e => e.UserCreatedOn).HasDefaultValue(0);
        builder.Property(e => e.ConversationId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
        builder.Property(e => e.ConversationCreatedOn).HasDefaultValue(0);
        builder.Property(e => e.Text).HasDefaultValue("");
        builder.Property(e => e.UpdatedOn).HasDefaultValue(0);
        builder.Property(e => e.CreatedOn).HasDefaultValue(0);
        builder.Property(e => e.IsDelivered).HasDefaultValue(0);
        builder.Property(e => e.IsRead).HasDefaultValue(0);
    }
}