using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QryloSocketAPI.Entities;

namespace QryloSocketAPI.Database.EntityConfigurations;

public class ConversationsConfiguration: IEntityTypeConfiguration<Conversations>
{
    public void Configure(EntityTypeBuilder<Conversations> builder)
    {
        builder.Property(e => e.Name).HasDefaultValue("");
        builder.Property(e => e.Avatar).HasDefaultValue("");
        builder.Property(e => e.CreatedOn).HasDefaultValue(0);
        builder.Property(e => e.IsPrivate).HasDefaultValue(0);
        builder.Property(e => e.CreatedBy).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
        builder.Property(e => e.CreatedByCreatedOn).HasDefaultValue(0);
    }
}