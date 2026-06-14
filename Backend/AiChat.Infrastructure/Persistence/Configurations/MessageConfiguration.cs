using AiChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations
{
    public class MessageConfiguration
     : IEntityTypeConfiguration<Message>
    {
        public void Configure(
            EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                                .ValueGeneratedNever();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.Role);

            builder.Property(x => x.CreatedAt);

            builder.Property(x => x.ConversationId);
        }
    }
}
