using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class FarmsDbContext : DbContext
    {
        private readonly string _connectionString;

        public FarmsDbContext(DbContextOptions<FarmsDbContext> options) : base(options)
        {
        }

        public DbSet<Farm> Users { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<CropSeason> CropSeasons { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply configurations from the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FarmsDbContext).Assembly);

            // Alternatively, you can apply configurations explicitly if needed
            //modelBuilder.ApplyConfiguration(new GameConfiguration());
            //modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
