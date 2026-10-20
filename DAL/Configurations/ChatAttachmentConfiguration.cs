using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

internal sealed class ChatAttachmentConfiguration : IEntityTypeConfiguration<ChatAttachment>
{
    public const string TableName = "ChatAttachments";

    public void Configure(EntityTypeBuilder<ChatAttachment> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.ChatMessage)
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.ChatMessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Data)
            .WithOne()
            .HasForeignKey<ChatAttachmentData>(d => d.Id);

        builder.Navigation(a => a.Data).IsRequired();

        builder.ConfigureAuditColumns();
    }
}
