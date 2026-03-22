
using Domain.Checklists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ChecklistConfiguration : IEntityTypeConfiguration<Checklist>
{
    public void Configure(EntityTypeBuilder<Checklist> builder)
    {
        //builder.ToTable("Checklists");

        //builder.HasKey(c => c.Id);

        //builder.Property(c => c.Name)
        //    .IsRequired()
        //    .HasMaxLength(200);

        //builder.Property(c => c.Email)
        //    .IsRequired()
        //    .HasMaxLength(256);

        //builder.HasIndex(c => c.Email)
        //    .IsUnique()
        //    ;
 
    }
}

