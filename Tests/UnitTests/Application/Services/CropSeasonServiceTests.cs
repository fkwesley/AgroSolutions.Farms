using Application.DTO.CropSeason;
using Application.Exceptions;
using Application.Mappings;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.Application.Services
{
    public class CropSeasonServiceTests
    {
        private readonly Mock<ICropSeasonRepository> _mockCropSeasonRepository;
        private readonly Mock<IFieldRepository> _mockFieldRepository;
        private readonly Mock<ILogger<CropSeasonService>> _mockLogger;
        private readonly CropSeasonService _cropSeasonService;

        public CropSeasonServiceTests()
        {
            _mockCropSeasonRepository = new Mock<ICropSeasonRepository>();
            _mockFieldRepository = new Mock<IFieldRepository>();
            _mockLogger = new Mock<ILogger<CropSeasonService>>();
            _cropSeasonService = new CropSeasonService(
                _mockCropSeasonRepository.Object,
                _mockFieldRepository.Object,
                _mockLogger.Object);
        }

        #region GetAllCropSeasonsAsync Tests

        [Fact]
        public async Task GetAllCropSeasonsAsync_ShouldReturnAllCropSeasons()
        {
            // Arrange
            var cropSeasons = new List<CropSeason>
            {
                CreateValidCropSeason(1, 1),
                CreateValidCropSeason(2, 1)
            };

            _mockCropSeasonRepository
                .Setup(r => r.GetAllCropSeasonsAsync())
                .ReturnsAsync(cropSeasons);

            // Act
            var result = await _cropSeasonService.GetAllCropSeasonsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCropSeasonRepository.Verify(r => r.GetAllCropSeasonsAsync(), Times.Once);
        }

        #endregion

        #region GetCropSeasonByIdAsync Tests

        [Fact]
        public async Task GetCropSeasonByIdAsync_WithValidId_ShouldReturnCropSeason()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason(1, 1);
            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(cropSeason);

            // Act
            var result = await _cropSeasonService.GetCropSeasonByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(CropType.Soybean, result.CropType);
        }

        [Fact]
        public async Task GetCropSeasonByIdAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(999))
                .ReturnsAsync((CropSeason?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cropSeasonService.GetCropSeasonByIdAsync(999));
        }

        #endregion

        #region GetCropSeasonsByFieldIdAsync Tests

        [Fact]
        public async Task GetCropSeasonsByFieldIdAsync_WithValidFieldId_ShouldReturnCropSeasons()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            var cropSeasons = new List<CropSeason>
            {
                CreateValidCropSeason(1, 1),
                CreateValidCropSeason(2, 1)
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonsByFieldIdAsync(1))
                .ReturnsAsync(cropSeasons);

            // Act
            var result = await _cropSeasonService.GetCropSeasonsByFieldIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCropSeasonsByFieldIdAsync_WithInvalidFieldId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(999))
                .ReturnsAsync((Field?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cropSeasonService.GetCropSeasonsByFieldIdAsync(999));
        }

        #endregion

        #region AddCropSeasonAsync Tests

        [Fact]
        public async Task AddCropSeasonAsync_WithValidData_ShouldAddCropSeason()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            var request = new AddCropSeasonRequest
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                CreatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            _mockCropSeasonRepository
                .Setup(r => r.HasDateConflictAsync(1, request.PlantingDate, request.ExpectedHarvestDate, null))
                .ReturnsAsync(false);

            _mockCropSeasonRepository
                .Setup(r => r.AddCropSeasonAsync(It.IsAny<CropSeason>()))
                .ReturnsAsync((CropSeason cs) =>
                {
                    cs.Id = 1;
                    return cs;
                });

            // Act
            var result = await _cropSeasonService.AddCropSeasonAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.FieldId);
            _mockCropSeasonRepository.Verify(r => r.AddCropSeasonAsync(It.IsAny<CropSeason>()), Times.Once);
        }

        [Fact]
        public async Task AddCropSeasonAsync_WithNonExistentField_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var request = new AddCropSeasonRequest
            {
                FieldId = 999,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                CreatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(999))
                .ReturnsAsync((Field?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cropSeasonService.AddCropSeasonAsync(request));
        }

        [Fact]
        public async Task AddCropSeasonAsync_WithInactiveField_ShouldThrowValidationException()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            field.Deactivate();

            var request = new AddCropSeasonRequest
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                CreatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _cropSeasonService.AddCropSeasonAsync(request));
            Assert.Contains("inactive field", exception.Message);
        }

        [Fact]
        public async Task AddCropSeasonAsync_WithDateConflict_ShouldThrowValidationException()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            var request = new AddCropSeasonRequest
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                CreatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            _mockCropSeasonRepository
                .Setup(r => r.HasDateConflictAsync(1, request.PlantingDate, request.ExpectedHarvestDate, null))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _cropSeasonService.AddCropSeasonAsync(request));
            Assert.Contains("not available", exception.Message);
            Assert.Contains("overlapping", exception.Message);
        }

        [Fact]
        public async Task AddCropSeasonAsync_WithValidHarvestDate_ShouldCreateFinishedCropSeason()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            var plantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120));
            var harvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

            var request = new AddCropSeasonRequest
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = plantingDate,
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                HarvestDate = harvestDate, // Informando data de colheita passada
                CreatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            _mockCropSeasonRepository
                .Setup(r => r.HasDateConflictAsync(1, request.PlantingDate, request.ExpectedHarvestDate, null))
                .ReturnsAsync(false);

            _mockCropSeasonRepository
                .Setup(r => r.AddCropSeasonAsync(It.IsAny<CropSeason>()))
                .ReturnsAsync((CropSeason cs) =>
                {
                    cs.Id = 1;
                    return cs;
                });

            // Act
            var result = await _cropSeasonService.AddCropSeasonAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(harvestDate, result.HarvestDate);
            Assert.Equal(CropSeasonStatus.Finished.ToString(), result.Status); // Status deve ser Finished
        }

        [Fact]
        public async Task AddCropSeasonAsync_WithHarvestDateToday_ShouldCreateFinishedCropSeason()
        {
            // Arrange
            var field = CreateValidField(1, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var request = new AddCropSeasonRequest
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                HarvestDate = today, // Data de colheita = hoje
                CreatedBy = "user123"
            };

            _mockFieldRepository
                .Setup(r => r.GetFieldByIdAsync(1))
                .ReturnsAsync(field);

            _mockCropSeasonRepository
                .Setup(r => r.HasDateConflictAsync(1, request.PlantingDate, request.ExpectedHarvestDate, null))
                .ReturnsAsync(false);

            _mockCropSeasonRepository
                .Setup(r => r.AddCropSeasonAsync(It.IsAny<CropSeason>()))
                .ReturnsAsync((CropSeason cs) =>
                {
                    cs.Id = 1;
                    return cs;
                });

            // Act
            var result = await _cropSeasonService.AddCropSeasonAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(today, result.HarvestDate);
            Assert.Equal(CropSeasonStatus.Finished.ToString(), result.Status);
        }

        #endregion

        #region UpdateCropSeasonAsync Tests

        [Fact]
        public async Task UpdateCropSeasonAsync_WithValidData_ShouldUpdateCropSeason()
        {
            // Arrange
            var existingCropSeason = CreateValidCropSeason(1, 1);
            var newExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(135));
            var request = new UpdateCropSeasonRequest
            {
                Id = 1,
                CropType = CropType.Corn,
                ExpectedHarvestDate = newExpectedHarvestDate,
                UpdatedBy = "user123"
            };

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(existingCropSeason);

            _mockCropSeasonRepository
                .Setup(r => r.HasDateConflictAsync(1, existingCropSeason.PlantingDate, newExpectedHarvestDate, 1))
                .ReturnsAsync(false);

            _mockCropSeasonRepository
                .Setup(r => r.UpdateCropSeasonAsync(It.IsAny<CropSeason>()))
                .ReturnsAsync((CropSeason cs) => cs);

            // Act
            var result = await _cropSeasonService.UpdateCropSeasonAsync(request);

            // Assert
            Assert.NotNull(result);
            _mockCropSeasonRepository.Verify(r => r.UpdateCropSeasonAsync(It.IsAny<CropSeason>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCropSeasonAsync_WithNonExistentCropSeason_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var request = new UpdateCropSeasonRequest
            {
                Id = 999,
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(135)),
                UpdatedBy = "user123"
            };

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(999))
                .ReturnsAsync((CropSeason?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cropSeasonService.UpdateCropSeasonAsync(request));
        }

        [Fact]
        public async Task UpdateCropSeasonAsync_WithActiveCropSeason_ShouldThrowValidationException()
        {
            // Arrange
            // Cria uma safra com PlantingDate no passado para ter Status = Active automaticamente
            var existingCropSeason = new CropSeason
            {
                Id = 1,
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(110)),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            var request = new UpdateCropSeasonRequest
            {
                Id = 1,
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(135)),
                UpdatedBy = "user123"
            };

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(existingCropSeason);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _cropSeasonService.UpdateCropSeasonAsync(request));
            Assert.Contains("Only planned crop seasons", exception.Message);
        }

        [Fact]
        public async Task UpdateCropSeasonAsync_WithDateConflict_ShouldThrowValidationException()
        {
            // Arrange
            var existingCropSeason = CreateValidCropSeason(1, 1);
            var newExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(135));
            var request = new UpdateCropSeasonRequest
            {
                Id = 1,
                ExpectedHarvestDate = newExpectedHarvestDate,
                UpdatedBy = "user123"
            };

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(existingCropSeason);

            _mockCropSeasonRepository
                .Setup(r => r.HasDateConflictAsync(1, existingCropSeason.PlantingDate, newExpectedHarvestDate, 1))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _cropSeasonService.UpdateCropSeasonAsync(request));
            Assert.Contains("not available", exception.Message);
            Assert.Contains("overlapping", exception.Message);
        }

        #endregion

        #region StartPlantingAsync Tests

        [Fact]
        public async Task StartPlantingAsync_WithValidCropSeason_ShouldStartPlanting()
        {
            // Arrange
            // Cria safra com PlantingDate = amanhã (Planned), pode iniciar plantio antecipado
            var cropSeason = new CropSeason
            {
                Id = 1,
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), // Amanhã = Planned
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(121)),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(cropSeason);

            _mockCropSeasonRepository
                .Setup(r => r.UpdateCropSeasonAsync(It.IsAny<CropSeason>()))
                .ReturnsAsync((CropSeason cs) => cs);

            // Act
            var result = await _cropSeasonService.StartPlantingAsync(1, "user123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(CropSeasonStatus.Active.ToString(), result.Status);
            _mockCropSeasonRepository.Verify(r => r.UpdateCropSeasonAsync(It.IsAny<CropSeason>()), Times.Once);
        }

        [Fact]
        public async Task StartPlantingAsync_WithNonExistentCropSeason_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(999))
                .ReturnsAsync((CropSeason?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cropSeasonService.StartPlantingAsync(999, "user123"));
        }

        #endregion

        #region FinishHarvestAsync Tests

        [Fact]
        public async Task FinishHarvestAsync_WithValidData_ShouldFinishHarvest()
        {
            // Arrange
            // Cria uma safra já ativa (PlantingDate no passado)
            var cropSeason = new CropSeason
            {
                Id = 1,
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };

            var harvestDate = DateOnly.FromDateTime(DateTime.UtcNow);

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(cropSeason);

            _mockCropSeasonRepository
                .Setup(r => r.UpdateCropSeasonAsync(It.IsAny<CropSeason>()))
                .ReturnsAsync((CropSeason cs) => cs);

            // Act
            var result = await _cropSeasonService.FinishHarvestAsync(1, harvestDate, "user123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(CropSeasonStatus.Finished.ToString(), result.Status);
            Assert.NotNull(result.HarvestDate);
        }

        #endregion

        #region DeleteCropSeasonAsync Tests

        [Fact]
        public async Task DeleteCropSeasonAsync_WithPlannedCropSeason_ShouldDelete()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason(1, 1);

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(cropSeason);

            _mockCropSeasonRepository
                .Setup(r => r.DeleteCropSeasonAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _cropSeasonService.DeleteCropSeasonAsync(1);

            // Assert
            Assert.True(result);
            _mockCropSeasonRepository.Verify(r => r.DeleteCropSeasonAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteCropSeasonAsync_WithActiveCropSeason_ShouldThrowValidationException()
        {
            // Arrange
            // Cria uma safra com PlantingDate no passado para ter Status = Active automaticamente
            var cropSeason = new CropSeason
            {
                Id = 1,
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(110)),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            _mockCropSeasonRepository
                .Setup(r => r.GetCropSeasonByIdAsync(1))
                .ReturnsAsync(cropSeason);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _cropSeasonService.DeleteCropSeasonAsync(1));
            Assert.Contains("Cannot delete active crop season", exception.Message);
        }

        #endregion

        #region Helper Methods

        private CropSeason CreateValidCropSeason(int id, int fieldId)
        {
            return new CropSeason
            {
                Id = id,
                FieldId = fieldId,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), // Data futura = Planned
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                // Status é calculado automaticamente baseado nas datas
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
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}
