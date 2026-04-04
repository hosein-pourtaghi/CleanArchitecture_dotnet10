
using Domain.Checklists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ChecklistConfiguration : IEntityTypeConfiguration<Checklist>
{
    public void Configure(EntityTypeBuilder<Checklist> builder)
    {   
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(255);
          
        // Versioning
        builder.Property(c => c.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(c => c.LastModified)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasMany(c => c.Groups)
            .WithOne(g => g.Checklist)
            .HasForeignKey(g => g.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.Title); 

    }
}

