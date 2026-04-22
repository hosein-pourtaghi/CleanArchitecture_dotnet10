// FileStorage.Infrastructure/Persistence/Configurations/FileAttachmentConfiguration.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileStorage.Infrastructure.Persistence.Configurations;

public class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.ToTable("FileAttachments", "FileStorage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.StoredFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FileExtension)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.AccessLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Bucket)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.OwnerType)
            .HasMaxLength(100);

        builder.Property(x => x.OwnerProperty)
            .HasMaxLength(100);

        builder.Property(x => x.ThumbnailPath)
            .HasMaxLength(1000);

        builder.Property(x => x.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Checksum)
            .HasMaxLength(64);

        // Indexes
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.OwnerType);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.AccessLevel);
        builder.HasIndex(x => x.Bucket);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.IsDeleted);

        // Composite indexes
        builder.HasIndex(x => new { x.OwnerId, x.OwnerType });
        builder.HasIndex(x => new { x.Bucket, x.IsDeleted });
    }
}
