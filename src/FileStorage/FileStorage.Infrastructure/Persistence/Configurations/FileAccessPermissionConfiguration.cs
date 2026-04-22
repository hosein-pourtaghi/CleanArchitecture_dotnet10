// FileStorage.Infrastructure/Persistence/Configurations/FileAccessPermissionConfiguration.cs
using FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileStorage.Infrastructure.Persistence.Configurations;

public class FileAccessPermissionConfiguration : IEntityTypeConfiguration<FileAccessPermission>
{
    public void Configure(EntityTypeBuilder<FileAccessPermission> builder)
    {
        builder.ToTable("FileAccessPermissions", "FileStorage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccessLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.FileAttachmentId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => new { x.FileAttachmentId, x.UserId }).IsUnique();

        // Relationships
        builder.HasOne(x => x.FileAttachment)
            .WithMany(f => f.Permissions)
            .HasForeignKey(x => x.FileAttachmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
