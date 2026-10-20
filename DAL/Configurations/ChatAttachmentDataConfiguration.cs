using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

internal sealed class ChatAttachmentDataConfiguration : IEntityTypeConfiguration<ChatAttachmentData>
{
    public void Configure(EntityTypeBuilder<ChatAttachmentData> builder)
    {
        builder.ToTable(ChatAttachmentConfiguration.TableName);
        builder.HasKey(d => d.Id);
    }
}
