using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class FieldConfiguration : IEntityTypeConfiguration<Field>
    {
        public void Configure(EntityTypeBuilder<Field> builder)
        {
            builder.ToTable("Fields");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .ValueGeneratedOnAdd();

            builder.Property(f => f.FarmId)
                .IsRequired();

            builder.Property(f => f.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(f => f.AreaHectares)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(f => f.Latitude)
                .HasPrecision(10, 7)
                .IsRequired();

            builder.Property(f => f.Longitude)
                .HasPrecision(10, 7)
                .IsRequired();

            builder.Property(f => f.IsActive)
                .HasDefaultValue(true);

            builder.Property(f => f.CreatedBy)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(f => f.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(f => f.UpdatedBy)
                .HasMaxLength(50);

            builder.Property(f => f.UpdatedAt);

            // Configure relationship with Farm
            builder.HasOne(f => f.Farm)
                .WithMany(farm => farm.Fields)
                .HasForeignKey(f => f.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with CropSeasons
            builder.HasMany(f => f.CropSeasons)
                .WithOne(cs => cs.Field)
                .HasForeignKey(cs => cs.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(f => f.FarmId);
            builder.HasIndex(f => f.IsActive);
        }
    }
}
