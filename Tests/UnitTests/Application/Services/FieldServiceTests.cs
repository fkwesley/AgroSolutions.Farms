using Application.DTO.Field;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.Application.Services
{
    public class FieldServiceTests
    {
        private readonly Mock<IFieldRepository> _mockFieldRepository;
        private readonly Mock<IFarmRepository> _mockFarmRepository;
        private readonly Mock<ILogger<FieldService>> _mockLogger;
        private readonly FieldService _fieldService;

        public FieldServiceTests()
        {
            _mockFieldRepository = new Mock<IFieldRepository>();
            _mockFarmRepository = new Mock<IFarmRepository>();
            _mockLogger = new Mock<ILogger<FieldService>>();
            _fieldService = new FieldService(
                _mockFieldRepository.Object,
                _mockFarmRepository.Object,
                _mockLogger.Object);
        }

        #region GetAllFieldsAsync Tests

        [Fact]
        public async Task GetAllFieldsAsync_ShouldReturnAllFields()
        {
            // Arrange
            var fields = new List<Field>
            {
                CreateValidField(1, 1),
                CreateValidField(2, 1)
            };

            _mockFieldRepository
                .Setup(r => r.GetAllFieldsAsync())
                .ReturnsAsync(fields);

            // Act
            var result = await _fieldService.GetAllFieldsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockFieldRepository.Verify(r => r.GetAllFieldsAsync(), Times.Once);
        }

        #endregion

        #region GetFieldByIdAsync Tests

        [Fact]
        public async Task GetFieldByIdAsync_WithValidId_ShouldReturnField()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            // Act
            var result = await _fieldService.GetFieldByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Field", result.Name);
        }

        [Fact]
        public async Task GetFieldByIdAsync_WithInvalidId_ShouldThrowValidationException()
        {
            // Arrange
            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(999))
                .ReturnsAsync((Field?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.GetFieldByIdAsync(999));
        }

        #endregion

        #region GetFieldsByFarmIdAsync Tests

        [Fact]
        public async Task GetFieldsByFarmIdAsync_WithValidFarmId_ShouldReturnFields()
        {
            // Arrange
            var farm = CreateValidFarm(1);
            var fields = new List<Field>
            {
                CreateValidField(1, 1),
                CreateValidField(2, 1)
            };

            _mockFarmRepository
                .Setup(r => r.GetAllFarmsAsync())
                .ReturnsAsync(new List<Farm> { farm });

            _mockFieldRepository
                .Setup(r => r.GetFieldsByFarmIdAsync(1))
                .ReturnsAsync(fields);

            // Act
            var result = await _fieldService.GetFieldsByFarmIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetFieldsByFarmIdAsync_WithInvalidFarmId_ShouldThrowValidationException()
        {
            // Arrange
            _mockFarmRepository
                .Setup(r => r.GetAllFarmsAsync())
                .ReturnsAsync(new List<Farm>());

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.GetFieldsByFarmIdAsync(999));
        }

        #endregion

        #region AddFieldAsync Tests

        [Fact]
        public async Task AddFieldAsync_WithValidData_ShouldAddField()
        {
            // Arrange
            var farm = CreateValidFarm(1);
            var request = new AddFieldRequest
            {
                FarmId = 1,
                Name = "New Field",
                AreaHectares = 100m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                CreatedBy = "user123"
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(farm);

            _mockFieldRepository
                .Setup(r => r.GetTotalFieldsAreaByFarmIdAsync(1, null))
                .ReturnsAsync(0m);

            _mockFieldRepository
                .Setup(r => r.AddFieldAsync(It.IsAny<Field>()))
                .ReturnsAsync((Field f) => { f.Id = 1; return f; });

            // Act
            var result = await _fieldService.AddFieldAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Field", result.Name);
            Assert.Equal(100m, result.AreaHectares);
            _mockFieldRepository.Verify(r => r.AddFieldAsync(It.IsAny<Field>()), Times.Once);
        }

        [Fact]
        public async Task AddFieldAsync_WithNonExistentFarm_ShouldThrowValidationException()
        {
            // Arrange
            var request = new AddFieldRequest
            {
                FarmId = 999,
                Name = "New Field",
                AreaHectares = 100m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                CreatedBy = "user123"
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(999))
                .ReturnsAsync((Farm?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.AddFieldAsync(request));
        }

        [Fact]
        public async Task AddFieldAsync_WithInactiveFarm_ShouldThrowValidationException()
        {
            // Arrange
            var farm = CreateValidFarm(1);
            farm.Deactivate();

            var request = new AddFieldRequest
            {
                FarmId = 1,
                Name = "New Field",
                AreaHectares = 100m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                CreatedBy = "user123"
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(farm);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.AddFieldAsync(request));
            Assert.Contains("inactive farm", exception.Message);
        }

        [Fact]
        public async Task AddFieldAsync_ExceedingFarmArea_ShouldThrowValidationException()
        {
            // Arrange
            var farm = CreateValidFarm(1);
            farm.TotalAreaHectares = 1000m;

            var request = new AddFieldRequest
            {
                FarmId = 1,
                Name = "New Field",
                AreaHectares = 600m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                CreatedBy = "user123"
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(farm);

            _mockFieldRepository
                .Setup(r => r.GetTotalFieldsAreaByFarmIdAsync(1, null))
                .ReturnsAsync(500m); // Already using 500ha

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.AddFieldAsync(request));
            Assert.Contains("exceeds farm total area", exception.Message);
        }

        #endregion

        #region UpdateFieldAsync Tests

        [Fact]
        public async Task UpdateFieldAsync_WithValidData_ShouldUpdateField()
        {
            // Arrange
            var existingField = CreateValidField(1, 1);
            var request = new UpdateFieldRequest
            {
                Id = 1,
                Name = "Updated Field",
                AreaHectares = 150m,
                Latitude = -23.6000m,
                Longitude = -46.7000m,
                UpdatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(existingField);

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(CreateValidFarm(1));

            _mockFieldRepository
                .Setup(r => r.GetTotalFieldsAreaByFarmIdAsync(1, 1))
                .ReturnsAsync(0m);

            _mockFieldRepository
                .Setup(r => r.UpdateFieldAsync(It.IsAny<Field>()))
                .ReturnsAsync((Field f) => f);

            // Act
            var result = await _fieldService.UpdateFieldAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Field", result.Name);
            _mockFieldRepository.Verify(r => r.UpdateFieldAsync(It.IsAny<Field>()), Times.Once);
        }

        [Fact]
        public async Task UpdateFieldAsync_WithNonExistentField_ShouldThrowValidationException()
        {
            // Arrange
            var request = new UpdateFieldRequest
            {
                Id = 999,
                Name = "Updated Field",
                AreaHectares = 150m,
                Latitude = -23.6000m,
                Longitude = -46.7000m,
                UpdatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(999))
                .ReturnsAsync((Field?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.UpdateFieldAsync(request));
        }

        [Fact]
        public async Task UpdateFieldAsync_ExceedingFarmArea_ShouldThrowValidationException()
        {
            // Arrange
            var existingField = CreateValidField(1, 1);
            existingField.AreaHectares = 100m;

            var farm = CreateValidFarm(1);
            farm.TotalAreaHectares = 1000m;

            var request = new UpdateFieldRequest
            {
                Id = 1,
                Name = "Updated Field",
                AreaHectares = 800m,
                Latitude = -23.6000m,
                Longitude = -46.7000m,
                UpdatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(existingField);

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(farm);

            _mockFieldRepository
                .Setup(r => r.GetTotalFieldsAreaByFarmIdAsync(1, 1))
                .ReturnsAsync(300m); // Other fields use 300ha

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.UpdateFieldAsync(request));
            Assert.Contains("exceeds farm total area", exception.Message);
        }

        #endregion

        #region DeleteFieldAsync Tests

        [Fact]
        public async Task DeleteFieldAsync_WithNoActiveCropSeasons_ShouldDelete()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            field.CropSeasons = new List<CropSeason>();

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            _mockFieldRepository
                .Setup(r => r.DeleteFieldAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _fieldService.DeleteFieldAsync(1);

            // Assert
            Assert.True(result);
            _mockFieldRepository.Verify(r => r.DeleteFieldAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteFieldAsync_WithActiveCropSeasons_ShouldThrowValidationException()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            field.CropSeasons = new List<CropSeason>
            {
                new CropSeason
                {
                    Id = 1,
                    FieldId = 1,
                    CropType = global::Domain.Enums.CropType.Soybean,
                    PlantingDate = DateTime.UtcNow.AddDays(10),
                    ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                    Status = global::Domain.Enums.CropSeasonStatus.Active,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.DeleteFieldAsync(1));
            Assert.Contains("has active crop seasons", exception.Message);
        }

        [Fact]
        public async Task DeleteFieldAsync_WithNonExistentField_ShouldThrowValidationException()
        {
            // Arrange
            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(999))
                .ReturnsAsync((Field?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _fieldService.DeleteFieldAsync(999));
        }

        #endregion

        #region Helper Methods

        private Farm CreateValidFarm(int id)
        {
            return new Farm
            {
                Id = id,
                ProducerId = "producer123",
                Name = "Test Farm",
                TotalAreaHectares = 1000m,
                IsActive = true,
                Location = new Location("São Paulo", "SP", "Brazil"),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };
        }

        private Field CreateValidField(int id, int farmId)
        {
            return new Field
            {
                Id = id,
                FarmId = farmId,
                Name = "Test Field",
                AreaHectares = 100m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow,
                CropSeasons = new List<CropSeason>()
            };
        }

        #endregion
    }
}
