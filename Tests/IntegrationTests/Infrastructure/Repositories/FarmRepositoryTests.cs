using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Tests.IntegrationTests.Common;
using Xunit;

namespace Tests.IntegrationTests.Infrastructure.Repositories
{
    public class FarmRepositoryTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly FarmsDbContext _context;
        private readonly IFarmRepository _repository;

        public FarmRepositoryTests(CustomWebApplicationFactory factory)
        {
            var options = new DbContextOptionsBuilder<FarmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FarmsDbContext(options);
            _repository = new FarmRepository(_context);

            SeedDatabase();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllFarmsAsync Tests

        [Fact]
        public async Task GetAllFarmsAsync_ShouldReturnAllFarms()
        {
            // Act
            var result = await _repository.GetAllFarmsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() >= 2);
        }

        #endregion

        #region GetFarmByIdAsync Tests

        [Fact]
        public async Task GetFarmByIdAsync_WithValidId_ShouldReturnFarm()
        {
            // Arrange
            var existingFarm = _context.Farms.First();

            // Act
            var result = await _repository.GetFarmByIdAsync(existingFarm.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingFarm.Id, result.Id);
            Assert.Equal(existingFarm.Name, result.Name);
        }

        [Fact]
        public async Task GetFarmByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetFarmByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddFarmAsync Tests

        [Fact]
        public async Task AddFarmAsync_WithValidData_ShouldAddFarm()
        {
            // Arrange
            var newFarm = new Farm
            {
                ProducerId = "producer999",
                Name = "New Farm",
                TotalAreaHectares = 1500m,
                IsActive = true,
                Location = new Location("Rio de Janeiro", "RJ", "Brazil"),
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddFarmAsync(newFarm);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);

            var savedFarm = await _context.Farms.FindAsync(result.Id);
            Assert.NotNull(savedFarm);
            Assert.Equal("New Farm", savedFarm.Name);
        }

        #endregion

        #region UpdateFarmAsync Tests

        [Fact]
        public async Task UpdateFarmAsync_WithValidData_ShouldUpdateFarm()
        {
            // Arrange
            var existingFarm = _context.Farms.First();
            var farmToUpdate = await _repository.GetFarmByIdAsync(existingFarm.Id);
            farmToUpdate!.Name = "Updated Farm Name";
            farmToUpdate.TotalAreaHectares = 2000m;

            // Act
            var result = await _repository.UpdateFarmAsync(farmToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Farm Name", result.Name);
            Assert.Equal(2000m, result.TotalAreaHectares);
        }

        #endregion

        #region DeleteFarmAsync Tests

        [Fact]
        public async Task DeleteFarmAsync_WithValidId_ShouldDeleteFarm()
        {
            // Arrange
            var farmToDelete = _context.Farms.First();

            // Act
            var result = await _repository.DeleteFarmAsync(farmToDelete.Id);

            // Assert
            Assert.True(result);

            var deletedFarm = await _context.Farms.FindAsync(farmToDelete.Id);
            Assert.Null(deletedFarm);
        }

        [Fact]
        public async Task DeleteFarmAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteFarmAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region FarmExistsAsync Tests

        [Fact]
        public async Task FarmExistsAsync_WithExistingFarmId_ShouldReturnTrue()
        {
            // Arrange
            var existingFarm = _context.Farms.First();

            // Act
            var exists = await _repository.FarmExistsAsync(existingFarm.Name);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task FarmExistsAsync_WithNonExistingFarmId_ShouldReturnFalse()
        {
            // Act
            var exists = await _repository.FarmExistsAsync("test Farm");

            // Assert
            Assert.False(exists);
        }

        #endregion

        #region Helper Methods

        private void SeedDatabase()
        {
            var farm1 = new Farm
            {
                ProducerId = "producer1",
                Name = "Farm 1",
                TotalAreaHectares = 1000m,
                IsActive = true,
                Location = new Location("São Paulo", "SP", "Brazil"),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            var farm2 = new Farm
            {
                ProducerId = "producer2",
                Name = "Farm 2",
                TotalAreaHectares = 1500m,
                IsActive = true,
                Location = new Location("Minas Gerais", "MG", "Brazil"),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            _context.Farms.AddRange(farm1, farm2);
            _context.SaveChanges();
        }

        #endregion
    }
}
