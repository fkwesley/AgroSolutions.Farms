using Application.DTO.Farm;
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
    public class FarmServiceTests
    {
        private readonly Mock<IFarmRepository> _mockFarmRepository;
        private readonly Mock<ILogger<FarmService>> _mockLogger;
        private readonly FarmService _farmService;

        public FarmServiceTests()
        {
            _mockFarmRepository = new Mock<IFarmRepository>();
            _mockLogger = new Mock<ILogger<FarmService>>();
            _farmService = new FarmService(_mockFarmRepository.Object, _mockLogger.Object);
        }

        #region GetAllFarmsAsync Tests

        [Fact]
        public async Task GetAllFarmsAsync_ShouldReturnAllFarms()
        {
            // Arrange
            var farms = new List<Farm>
            {
                CreateValidFarm(1, "producer1"),
                CreateValidFarm(2, "producer2")
            };

            _mockFarmRepository
                .Setup(r => r.GetAllFarmsAsync())
                .ReturnsAsync(farms);

            // Act
            var result = await _farmService.GetAllFarmsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockFarmRepository.Verify(r => r.GetAllFarmsAsync(), Times.Once);
        }

        #endregion

        #region GetFarmByIdAsync Tests

        [Fact]
        public async Task GetFarmByIdAsync_WithValidId_ShouldReturnFarm()
        {
            // Arrange
            var farm = CreateValidFarm(1, "producer1");
            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(farm);

            // Act
            var result = await _farmService.GetFarmByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.FarmId);
            Assert.Equal("Test Farm", result.FarmName);
        }

        [Fact]
        public async Task GetFarmByIdAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(999))
                .ReturnsAsync((Farm?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _farmService.GetFarmByIdAsync(999));
        }

        #endregion

        #region AddFarmAsync Tests

        [Fact]
        public async Task AddFarmAsync_WithValidData_ShouldAddFarm()
        {
            // Arrange
            var request = new AddFarmRequest
            {
                ProducerId = "producer123",
                Name = "New Farm",
                TotalAreaHectares = 1000m,
                Location = new global::Application.DTO.Common.LocationDto
                {
                    City = "São Paulo",
                    State = "SP",
                    Country = "Brazil"
                }
            };

            _mockFarmRepository
                .Setup(r => r.FarmExistsAsync("New Farm"))
                .ReturnsAsync(false);

            _mockFarmRepository
                .Setup(r => r.AddFarmAsync(It.IsAny<Farm>()))
                .ReturnsAsync((Farm f) => f);

            // Act
            var result = await _farmService.AddFarmAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Farm", result.FarmName);
            Assert.Equal(1000m, result.TotalAreaHectares);
            _mockFarmRepository.Verify(r => r.AddFarmAsync(It.IsAny<Farm>()), Times.Once);
        }

        [Fact]
        public async Task AddFarmAsync_WithExistingFarmId_ShouldThrowValidationException()
        {
            // Arrange
            var request = new AddFarmRequest
            {
                ProducerId = "producer123",
                Name = "New Farm",
                TotalAreaHectares = 1000m,
                Location = new global::Application.DTO.Common.LocationDto
                {
                    City = "São Paulo",
                    State = "SP",
                    Country = "Brazil"
                }
            };

            _mockFarmRepository
                .Setup(r => r.FarmExistsAsync("New Farm"))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _farmService.AddFarmAsync(request));
            Assert.Contains("already exists", exception.Message);
        }

        #endregion

        #region UpdateFarmAsync Tests

        [Fact]
        public async Task UpdateFarmAsync_WithValidData_ShouldUpdateFarm()
        {
            // Arrange
            var existingFarm = CreateValidFarm(1, "producer123");
            existingFarm.Fields = new List<Field>
            {
                CreateValidField(1, 1, 200m)
            };

            var request = new UpdateFarmRequest
            {
                FarmId = 1,
                FarmName = "Updated Farm",
                TotalAreaHectares = 1500m,
                Location = new global::Application.DTO.Common.LocationDto
                {
                    City = "Rio de Janeiro",
                    State = "RJ",
                    Country = "Brazil"
                },
                UpdatedBy = "user123"
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(existingFarm);

            _mockFarmRepository
                .Setup(r => r.UpdateFarmAsync(It.IsAny<Farm>()))
                .ReturnsAsync((Farm f) => f);

            // Act
            var result = await _farmService.UpdateFarmAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Farm", result.FarmName);
            _mockFarmRepository.Verify(r => r.UpdateFarmAsync(It.IsAny<Farm>()), Times.Once);
        }

        [Fact]
        public async Task UpdateFarmAsync_WithNonExistentFarm_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var request = new UpdateFarmRequest
            {
                FarmId = 999,
                FarmName = "Updated Farm",
                TotalAreaHectares = 1500m,
                Location = new global::Application.DTO.Common.LocationDto
                {
                    City = "Rio de Janeiro",
                    State = "RJ",
                    Country = "Brazil"
                },
                UpdatedBy = "user123"
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(999))
                .ReturnsAsync((Farm?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _farmService.UpdateFarmAsync(request));
        }

        [Fact]
        public async Task UpdateFarmAsync_WithAreaSmallerThanUsed_ShouldThrowValidationException()
        {
            // Arrange
            var existingFarm = CreateValidFarm(1, "producer123");
            existingFarm.Fields = new List<Field>
            {
                CreateValidField(1, 1, 500m),
                CreateValidField(2, 1, 300m)
            };

            var request = new UpdateFarmRequest
            {
                FarmId = 1,
                FarmName = "Updated Farm",
                TotalAreaHectares = 700m, // Less than used (800ha)
                Location = new global::Application.DTO.Common.LocationDto
                {
                    City = "Rio de Janeiro",
                    State = "RJ",
                    Country = "Brazil"
                },
                UpdatedBy = "user123"
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(existingFarm);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _farmService.UpdateFarmAsync(request));
            Assert.Contains("Cannot reduce farm area", exception.Message);
        }

        #endregion

        #region DeleteFarmAsync Tests

        [Fact]
        public async Task DeleteFarmAsync_WithNoActiveFields_ShouldDelete()
        {
            // Arrange
            var farm = CreateValidFarm(1, "producer123");
            var inactiveField = CreateValidField(1, 1, 100m);
            inactiveField.Deactivate();
            farm.Fields = new List<Field> { inactiveField };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(farm);

            _mockFarmRepository
                .Setup(r => r.DeleteFarmAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _farmService.DeleteFarmAsync(1);

            // Assert
            Assert.True(result);
            _mockFarmRepository.Verify(r => r.DeleteFarmAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteFarmAsync_WithActiveFields_ShouldThrowValidationException()
        {
            // Arrange
            var farm = CreateValidFarm(1, "producer123");
            farm.Fields = new List<Field>
            {
                CreateValidField(1, 1, 100m) // Active field
            };

            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(1))
                .ReturnsAsync(farm);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _farmService.DeleteFarmAsync(1));
            Assert.Contains("has active fields", exception.Message);
        }

        [Fact]
        public async Task DeleteFarmAsync_WithNonExistentFarm_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockFarmRepository
                .Setup(r => r.GetFarmByIdAsync(999))
                .ReturnsAsync((Farm?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _farmService.DeleteFarmAsync(999));
        }

        #endregion

        #region Helper Methods

        private Farm CreateValidFarm(int id, string producerId)
        {
            return new Farm
            {
                Id = id,
                ProducerId = producerId,
                Name = "Test Farm",
                TotalAreaHectares = 1000m,
                IsActive = true,
                Location = new Location("São Paulo", "SP", "Brazil"),
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow,
                Fields = new List<Field>()
            };
        }

        private Field CreateValidField(int id, int farmId, decimal area)
        {
            return new Field
            {
                Id = id,
                FarmId = farmId,
                Name = $"Field {id}",
                AreaHectares = area,
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
