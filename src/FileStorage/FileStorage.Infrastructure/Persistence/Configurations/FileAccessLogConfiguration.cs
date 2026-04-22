// FileStorage.Infrastructure/Persistence/Configurations/FileAccessLogConfiguration.cs
using FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileStorage.Infrastructure.Persistence.Configurations;

public class FileAccessLogConfiguration : IEntityTypeConfiguration<FileAccessLog>
{
    public void Configure(EntityTypeBuilder<FileAccessLog> builder)
    {
        builder.ToTable("FileAccessLogs", "FileStorage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.OwnerType)
            .HasMaxLength(100);

        builder.Property(x => x.RequestPath)
            .HasMaxLength(500);

        builder.Property(x => x.HttpMethod)
            .HasMaxLength(10);

        // Indexes
        builder.HasIndex(x => x.FileAttachmentId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.Success);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.FileAttachmentId, x.CreatedAt });
    }
}
