using AiChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations
{
    public class ConversationConfiguration
     : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(
        EntityTypeBuilder<Conversation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired();

            builder.HasMany(x => x.Messages)
                .WithOne()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade); 

            builder.Navigation(x => x.Messages)
                .UsePropertyAccessMode(
                    PropertyAccessMode.Field);
        } 
    }
}
