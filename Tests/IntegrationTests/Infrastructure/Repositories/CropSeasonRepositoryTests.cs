using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Tests.IntegrationTests.Common;
using Xunit;

namespace Tests.IntegrationTests.Infrastructure.Repositories
{
    public class CropSeasonRepositoryTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly FarmsDbContext _context;
        private readonly ICropSeasonRepository _repository;

        public CropSeasonRepositoryTests(CustomWebApplicationFactory factory)
        {
            var options = new DbContextOptionsBuilder<FarmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FarmsDbContext(options);
            _repository = new CropSeasonRepository(_context);

            // Seed database
            SeedDatabase();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllCropSeasonsAsync Tests

        [Fact]
        public async Task GetAllCropSeasonsAsync_ShouldReturnAllCropSeasons()
        {
            // Act
            var result = await _repository.GetAllCropSeasonsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() >= 2);
        }

        #endregion

        #region GetCropSeasonByIdAsync Tests

        [Fact]
        public async Task GetCropSeasonByIdAsync_WithValidId_ShouldReturnCropSeason()
        {
            // Arrange
            var existingCropSeason = _context.CropSeasons.First();

            // Act
            var result = await _repository.GetCropSeasonByIdAsync(existingCropSeason.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingCropSeason.Id, result.Id);
            Assert.Equal(existingCropSeason.CropType, result.CropType);
        }

        [Fact]
        public async Task GetCropSeasonByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetCropSeasonByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetCropSeasonsByFieldIdAsync Tests

        [Fact]
        public async Task GetCropSeasonsByFieldIdAsync_WithValidFieldId_ShouldReturnCropSeasons()
        {
            // Arrange
            var field = _context.Fields.First();

            // Act
            var result = await _repository.GetCropSeasonsByFieldIdAsync(field.Id);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, cs => Assert.Equal(field.Id, cs.FieldId));
        }

        [Fact]
        public async Task GetCropSeasonsByFieldIdAsync_WithInvalidFieldId_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetCropSeasonsByFieldIdAsync(999);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetCropSeasonsByStatusAsync Tests

        [Fact]
        public async Task GetCropSeasonsByStatusAsync_ShouldReturnCropSeasonsWithStatus()
        {
            // Act
            var result = await _repository.GetCropSeasonsByStatusAsync(CropSeasonStatus.Planned);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, cs => Assert.Equal(CropSeasonStatus.Planned, cs.Status));
        }

        #endregion

        #region GetOverdueCropSeasonsAsync Tests

        [Fact]
        public async Task GetOverdueCropSeasonsAsync_ShouldReturnOverdueCropSeasons()
        {
            // Arrange
            var field = _context.Fields.First();
            var overdueCropSeason = new CropSeason
            {
                FieldId = field.Id,
                CropType = CropType.Corn,
                PlantingDate = DateTime.UtcNow.AddDays(-150),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(-10),
                Status = CropSeasonStatus.Active,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };

            _context.CropSeasons.Add(overdueCropSeason);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetOverdueCropSeasonsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, cs => cs.Id == overdueCropSeason.Id);
        }

        #endregion

        #region AddCropSeasonAsync Tests

        [Fact]
        public async Task AddCropSeasonAsync_WithValidData_ShouldAddCropSeason()
        {
            // Arrange
            var field = _context.Fields.First();
            var newCropSeason = new CropSeason
            {
                FieldId = field.Id,
                CropType = CropType.Wheat,
                PlantingDate = DateTime.UtcNow.AddDays(20),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(140),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddCropSeasonAsync(newCropSeason);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);

            var savedCropSeason = await _context.CropSeasons.FindAsync(result.Id);
            Assert.NotNull(savedCropSeason);
            Assert.Equal(CropType.Wheat, savedCropSeason.CropType);
        }

        #endregion

        #region UpdateCropSeasonAsync Tests

        [Fact]
        public async Task UpdateCropSeasonAsync_WithValidData_ShouldUpdateCropSeason()
        {
            // Arrange
            var existingCropSeason = _context.CropSeasons.First();
            var newPlantingDate = DateTime.UtcNow.AddDays(25);
            
            var cropSeasonToUpdate = await _repository.GetCropSeasonByIdAsync(existingCropSeason.Id);
            cropSeasonToUpdate!.PlantingDate = newPlantingDate;
            cropSeasonToUpdate.ExpectedHarvestDate = newPlantingDate.AddDays(120);

            // Act
            var result = await _repository.UpdateCropSeasonAsync(cropSeasonToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newPlantingDate.Date, result.PlantingDate.Date);
        }

        #endregion

        #region DeleteCropSeasonAsync Tests

        [Fact]
        public async Task DeleteCropSeasonAsync_WithValidId_ShouldDeleteCropSeason()
        {
            // Arrange
            var cropSeasonToDelete = _context.CropSeasons.First();

            // Act
            var result = await _repository.DeleteCropSeasonAsync(cropSeasonToDelete.Id);

            // Assert
            Assert.True(result);

            var deletedCropSeason = await _context.CropSeasons.FindAsync(cropSeasonToDelete.Id);
            Assert.Null(deletedCropSeason);
        }

        [Fact]
        public async Task DeleteCropSeasonAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteCropSeasonAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region HasDateConflictAsync Tests

        [Fact]
        public async Task HasDateConflictAsync_WithOverlappingDates_ShouldReturnTrue()
        {
            // Arrange
            var field = _context.Fields.First();
            var existingCropSeason = new CropSeason
            {
                FieldId = field.Id,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(10),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddCropSeasonAsync(existingCropSeason);

            // Act - Tentando adicionar safra com datas sobrepostas
            var hasConflict = await _repository.HasDateConflictAsync(
                field.Id,
                DateTime.UtcNow.AddDays(50), // Overlap: dentro do período existente
                DateTime.UtcNow.AddDays(170));

            // Assert
            Assert.True(hasConflict);
        }

        [Fact]
        public async Task HasDateConflictAsync_WithNonOverlappingDates_ShouldReturnFalse()
        {
            // Arrange
            var field = _context.Fields.First();

            // Act - Datas não sobrepostas
            var hasConflict = await _repository.HasDateConflictAsync(
                field.Id,
                DateTime.UtcNow.AddDays(200), // Após as safras existentes
                DateTime.UtcNow.AddDays(320));

            // Assert
            Assert.False(hasConflict);
        }

        [Fact]
        public async Task HasDateConflictAsync_ExcludingCropSeasonId_ShouldReturnFalse()
        {
            // Arrange - Create a field without existing crop seasons to avoid conflicts
            var farm = _context.Farms.First();
            var testField = new Field
            {
                FarmId = farm.Id,
                Name = "Test Field for Exclusion",
                AreaHectares = 100m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                IsActive = true,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };
            _context.Fields.Add(testField);
            await _context.SaveChangesAsync();

            var existingCropSeason = new CropSeason
            {
                FieldId = testField.Id,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(10),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };

            var added = await _repository.AddCropSeasonAsync(existingCropSeason);

            // Act - Verificando conflito excluindo a própria safra (para updates)
            var hasConflict = await _repository.HasDateConflictAsync(
                testField.Id,
                DateTime.UtcNow.AddDays(15), // Mesmas datas da safra existente
                DateTime.UtcNow.AddDays(135),
                excludeCropSeasonId: added.Id); // Exclui ela mesma

            // Assert
            Assert.False(hasConflict); // Não deve ter conflito consigo mesma
        }

        [Fact]
        public async Task HasDateConflictAsync_WithFinishedCropSeason_ShouldReturnFalse()
        {
            // Arrange
            var field = _context.Fields.First();
            var finishedCropSeason = new CropSeason
            {
                FieldId = field.Id,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(-120),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(-10),
                HarvestDate = DateTime.UtcNow.AddDays(-5),
                Status = CropSeasonStatus.Finished, // Finished não ocupa o campo
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddCropSeasonAsync(finishedCropSeason);

            // Act - Tentando adicionar safra no mesmo período de uma finalizada
            var hasConflict = await _repository.HasDateConflictAsync(
                field.Id,
                DateTime.UtcNow.AddDays(-100),
                DateTime.UtcNow.AddDays(-20));

            // Assert
            Assert.False(hasConflict); // Safras finalizadas não causam conflito
        }

        #endregion

        #region Helper Methods

        private void SeedDatabase()
        {
            var farm = new Farm
            {
                Name = "Test Farm",
                ProducerId = "producer123",
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
                AreaHectares = 100m,
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
                AreaHectares = 150m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            _context.Fields.AddRange(field1, field2);
            _context.SaveChanges();

            var cropSeason1 = new CropSeason
            {
                FieldId = field1.Id,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(10),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            var cropSeason2 = new CropSeason
            {
                FieldId = field2.Id,
                CropType = CropType.Corn,
                PlantingDate = DateTime.UtcNow.AddDays(15),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(135),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            _context.CropSeasons.AddRange(cropSeason1, cropSeason2);
            _context.SaveChanges();
        }

        #endregion
    }
}
