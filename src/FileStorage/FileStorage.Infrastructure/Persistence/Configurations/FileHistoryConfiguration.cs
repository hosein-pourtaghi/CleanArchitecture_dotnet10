// FileStorage.Infrastructure/Persistence/Configurations/FileHistoryConfiguration.cs
using FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileStorage.Infrastructure.Persistence.Configurations;

public class FileHistoryConfiguration : IEntityTypeConfiguration<FileHistory>
{
    public void Configure(EntityTypeBuilder<FileHistory> builder)
    {
        builder.ToTable("FileHistories", "FileStorage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.OwnerType)
            .HasMaxLength(100);

        builder.Property(x => x.OwnerProperty)
            .HasMaxLength(100);

        builder.Property(x => x.ChangeReason)
            .HasMaxLength(500);

        builder.Property(x => x.ChangeDetailsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.FileAttachmentId);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.OwnerType);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.CreatedAt);

        // Relationship
        builder.HasOne(x => x.FileAttachment)
            .WithMany(f => f.History)
            .HasForeignKey(x => x.FileAttachmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
