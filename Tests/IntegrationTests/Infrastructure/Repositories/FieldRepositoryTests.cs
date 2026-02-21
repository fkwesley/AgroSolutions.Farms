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
    public class FieldRepositoryTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly FarmsDbContext _context;
        private readonly IFieldRepository _repository;

        public FieldRepositoryTests(CustomWebApplicationFactory factory)
        {
            var options = new DbContextOptionsBuilder<FarmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FarmsDbContext(options);
            _repository = new FieldRepository(_context);

            SeedDatabase();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllFieldsAsync Tests

        [Fact]
        public async Task GetAllFieldsAsync_ShouldReturnAllFields()
        {
            // Act
            var result = await _repository.GetAllFieldsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() >= 2);
        }

        #endregion

        #region GetFieldByIdAsync Tests

        [Fact]
        public async Task GetFieldByIdAsync_WithValidId_ShouldReturnField()
        {
            // Arrange
            var existingField = _context.Fields.First();

            // Act
            var result = await _repository.GetFieldByIdAsync(existingField.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingField.Id, result.Id);
            Assert.Equal(existingField.Name, result.Name);
        }

        [Fact]
        public async Task GetFieldByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetFieldByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetFieldsByFarmIdAsync Tests

        [Fact]
        public async Task GetFieldsByFarmIdAsync_WithValidFarmId_ShouldReturnFields()
        {
            // Arrange
            var farm = _context.Farms.First();

            // Act
            var result = await _repository.GetFieldsByFarmIdAsync(farm.Id);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, f => Assert.Equal(farm.Id, f.FarmId));
        }

        [Fact]
        public async Task GetFieldsByFarmIdAsync_WithInvalidFarmId_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetFieldsByFarmIdAsync(999);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region AddFieldAsync Tests

        [Fact]
        public async Task AddFieldAsync_WithValidData_ShouldAddField()
        {
            // Arrange
            var farm = _context.Farms.First();
            var newField = new Field
            {
                FarmId = farm.Id,
                Name = "New Field",
                AreaHectares = 150m,
                Latitude = -23.6000m,
                Longitude = -46.7000m,
                IsActive = true,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddFieldAsync(newField);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);

            var savedField = await _context.Fields.FindAsync(result.Id);
            Assert.NotNull(savedField);
            Assert.Equal("New Field", savedField.Name);
        }

        #endregion

        #region UpdateFieldAsync Tests

        [Fact]
        public async Task UpdateFieldAsync_WithValidData_ShouldUpdateField()
        {
            // Arrange
            var existingField = _context.Fields.First();
            var fieldToUpdate = await _repository.GetFieldByIdAsync(existingField.Id);
            fieldToUpdate!.Name = "Updated Field Name";
            fieldToUpdate.AreaHectares = 250m;

            // Act
            var result = await _repository.UpdateFieldAsync(fieldToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Field Name", result.Name);
            Assert.Equal(250m, result.AreaHectares);
        }

        #endregion

        #region DeleteFieldAsync Tests

        [Fact]
        public async Task DeleteFieldAsync_WithValidId_ShouldDeleteField()
        {
            // Arrange
            var fieldToDelete = _context.Fields.First();

            // Act
            var result = await _repository.DeleteFieldAsync(fieldToDelete.Id);

            // Assert
            Assert.True(result);

            var deletedField = await _context.Fields.FindAsync(fieldToDelete.Id);
            Assert.Null(deletedField);
        }

        [Fact]
        public async Task DeleteFieldAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteFieldAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region FieldExistsAsync Tests

        [Fact]
        public async Task FieldExistsAsync_WithExistingFieldName_ShouldReturnTrue()
        {
            // Arrange
            var existingField = _context.Fields.First();

            // Act
            var exists = await _repository.FieldExistsAsync(existingField.Name);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task FieldExistsAsync_WithNonExistingFieldName_ShouldReturnFalse()
        {
            // Act
            var exists = await _repository.FieldExistsAsync("Non Existing Field");

            // Assert
            Assert.False(exists);
        }

        #endregion

        #region GetTotalFieldsAreaByFarmIdAsync Tests

        [Fact]
        public async Task GetTotalFieldsAreaByFarmIdAsync_ShouldReturnSum()
        {
            // Arrange
            var farm = _context.Farms.First();

            // Act
            var totalArea = await _repository.GetTotalFieldsAreaByFarmIdAsync(farm.Id);

            // Assert
            Assert.True(totalArea > 0);
        }

        [Fact]
        public async Task GetTotalFieldsAreaByFarmIdAsync_ExcludingField_ShouldReturnSumWithoutExcluded()
        {
            // Arrange
            var farm = _context.Farms.First();
            var fieldToExclude = _context.Fields.First(f => f.FarmId == farm.Id);

            var allFieldsArea = await _repository.GetTotalFieldsAreaByFarmIdAsync(farm.Id);

            // Act
            var areaExcludingOne = await _repository.GetTotalFieldsAreaByFarmIdAsync(farm.Id, fieldToExclude.Id);

            // Assert
            Assert.Equal(allFieldsArea - fieldToExclude.AreaHectares, areaExcludingOne);
        }

        [Fact]
        public async Task GetTotalFieldsAreaByFarmIdAsync_WithNoFields_ShouldReturnZero()
        {
            // Act
            var totalArea = await _repository.GetTotalFieldsAreaByFarmIdAsync(999);

            // Assert
            Assert.Equal(0, totalArea);
        }

        #endregion

        #region Helper Methods

        private void SeedDatabase()
        {
            var farm = new Farm
            {
                ProducerId = "producer1",
                Name = "Test Farm",
                TotalAreaHectares = 1000m,
                IsActive = true,
                Location = new Location("São Paulo", "SP", "Brazil"),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            _context.Farms.Add(farm);
            _context.SaveChanges();

            var field1 = new Field
            {
                FarmId = farm.Id,
                Name = "Field 1",
                AreaHectares = 200m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            var field2 = new Field
            {
                FarmId = farm.Id,
                Name = "Field 2",
                AreaHectares = 300m,
                Latitude = -23.6000m,
                Longitude = -46.7000m,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            _context.Fields.AddRange(field1, field2);
            _context.SaveChanges();
        }

        #endregion
    }
}
