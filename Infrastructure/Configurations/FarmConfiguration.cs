using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class FarmConfiguration : IEntityTypeConfiguration<Farm>
    {
        public void Configure(EntityTypeBuilder<Farm> builder)
        {
            builder.ToTable("Farms");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .ValueGeneratedOnAdd();

            builder.Property(f => f.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(f => f.ProducerId)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(f => f.TotalAreaHectares)
                .HasPrecision(18, 2)
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

            // Configure Location as an owned type (Value Object)
            builder.OwnsOne(f => f.Location, location =>
            {
                location.Property(l => l.City)
                    .HasColumnName("City")
                    .HasMaxLength(100)
                    .IsRequired();

                location.Property(l => l.State)
                    .HasColumnName("State")
                    .HasMaxLength(100)
                    .IsRequired();

                location.Property(l => l.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            // Configure relationship with Fields
            builder.HasMany(f => f.Fields)
                .WithOne(field => field.Farm)
                .HasForeignKey(field => field.FarmId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
