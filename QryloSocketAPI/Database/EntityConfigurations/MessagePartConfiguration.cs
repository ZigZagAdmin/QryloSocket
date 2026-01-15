using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QryloSocketAPI.Entities;

namespace QryloSocketAPI.Database.EntityConfigurations;

public class MessagePartConfiguration: IEntityTypeConfiguration<MessagePart>
{
    public void Configure(EntityTypeBuilder<MessagePart> builder)
    {
        builder.Property(e => e.MessageId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
        builder.Property(e => e.MessageCreatedOn).HasDefaultValue(0);
        builder.Property(e => e.Type).HasDefaultValueSql("'Text'::message_part_types");
        builder.Property(e => e.Order).HasDefaultValue(0);
        builder.Property(e => e.Content).HasDefaultValue("");
        builder.Property(e => e.CreatedOn).HasDefaultValue(0);
    }
}