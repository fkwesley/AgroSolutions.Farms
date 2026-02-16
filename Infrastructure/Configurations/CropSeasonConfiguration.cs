using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class CropSeasonConfiguration : IEntityTypeConfiguration<CropSeason>
    {
        public void Configure(EntityTypeBuilder<CropSeason> builder)
        {
            builder.ToTable("CropSeasons");

            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Id)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(cs => cs.FieldId)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(cs => cs.CropType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(cs => cs.PlantingDate)
                .IsRequired();

            builder.Property(cs => cs.ExpectedHarvestDate)
                .IsRequired();

            builder.Property(cs => cs.HarvestDate)
                .IsRequired(false);

            builder.Property(cs => cs.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(CropSeasonStatus.Planned);

            builder.Property(cs => cs.CreatedBy)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(cs => cs.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(cs => cs.UpdatedBy)
                .HasMaxLength(50);

            builder.Property(cs => cs.UpdatedAt);

            // Configure relationship with Field
            builder.HasOne(cs => cs.Field)
                .WithMany(f => f.CropSeasons)
                .HasForeignKey(cs => cs.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(cs => cs.FieldId);
            builder.HasIndex(cs => cs.Status);
            builder.HasIndex(cs => cs.PlantingDate);
            builder.HasIndex(cs => cs.ExpectedHarvestDate);
        }
    }
}
