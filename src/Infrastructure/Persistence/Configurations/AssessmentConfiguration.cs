
using Domain.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        // Properties
        builder.Property(a => a.ChecklistId)
            .IsRequired();

        builder.Property(a => a.ChecklistVersion)
            .IsRequired();

        builder.Property(a => a.AssessmentDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(a => a.Checklist)
            .WithMany()
            .HasForeignKey(a => a.ChecklistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Answers)
            .WithOne(a => a.Assessment)
            .HasForeignKey(a => a.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.ChecklistId);
        builder.HasIndex(a => a.ChecklistVersion);
        builder.HasIndex(a => a.AssessmentDate);

    }
}

