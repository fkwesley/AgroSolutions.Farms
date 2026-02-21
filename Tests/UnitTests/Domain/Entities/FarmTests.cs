using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace Tests.UnitTests.Domain.Entities
{
    public class FarmTests
    {
        #region Constructor Tests

        [Fact]
        public void Farm_WithValidData_ShouldCreate()
        {
            // Arrange & Act
            var farm = new Farm
            {
                ProducerId = "producer123",
                Name = "Test Farm",
                TotalAreaHectares = 1000m,
                Location = new Location("São Paulo", "SP", "Brazil"),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal("producer123", farm.ProducerId);
            Assert.Equal("Test Farm", farm.Name);
            Assert.Equal(1000m, farm.TotalAreaHectares);
            Assert.True(farm.IsActive);
        }

        [Fact]
        public void Farm_WithZeroArea_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var farm = new Farm
                {
                    ProducerId = "producer123",
                    Name = "Test Farm",
                    TotalAreaHectares = 0m,
                    Location = new Location("São Paulo", "SP", "Brazil"),
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Farm total area must be greater than zero", exception.Message);
        }

        [Fact]
        public void Farm_WithNegativeArea_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var farm = new Farm
                {
                    ProducerId = "producer123",
                    Name = "Test Farm",
                    TotalAreaHectares = -100m,
                    Location = new Location("São Paulo", "SP", "Brazil"),
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Farm total area must be greater than zero", exception.Message);
        }

        #endregion

        #region GetTotalFieldsArea Tests

        [Fact]
        public void GetTotalFieldsArea_WithNoFields_ShouldReturnZero()
        {
            // Arrange
            var farm = CreateValidFarm();

            // Act
            var totalArea = farm.GetTotalFieldsArea();

            // Assert
            Assert.Equal(0m, totalArea);
        }

        [Fact]
        public void GetTotalFieldsArea_WithMultipleFields_ShouldReturnSum()
        {
            // Arrange
            var farm = CreateValidFarm();
            farm.Fields = new List<Field>
            {
                CreateValidField(1, farm.Id, 200m),
                CreateValidField(2, farm.Id, 300m),
                CreateValidField(3, farm.Id, 150m)
            };

            // Act
            var totalArea = farm.GetTotalFieldsArea();

            // Assert
            Assert.Equal(650m, totalArea);
        }

        #endregion

        #region GetAvailableArea Tests

        [Fact]
        public void GetAvailableArea_WithNoFields_ShouldReturnTotalArea()
        {
            // Arrange
            var farm = CreateValidFarm();
            farm.TotalAreaHectares = 1000m;

            // Act
            var availableArea = farm.GetAvailableArea();

            // Assert
            Assert.Equal(1000m, availableArea);
        }

        [Fact]
        public void GetAvailableArea_WithSomeFields_ShouldReturnRemaining()
        {
            // Arrange
            var farm = CreateValidFarm();
            farm.TotalAreaHectares = 1000m;
            farm.Fields = new List<Field>
            {
                CreateValidField(1, farm.Id, 300m),
                CreateValidField(2, farm.Id, 200m)
            };

            // Act
            var availableArea = farm.GetAvailableArea();

            // Assert
            Assert.Equal(500m, availableArea);
        }

        #endregion

        #region ValidateAreaForNewField Tests

        [Fact]
        public void ValidateAreaForNewField_WithSufficientArea_ShouldNotThrow()
        {
            // Arrange
            var farm = CreateValidFarm();
            farm.TotalAreaHectares = 1000m;
            farm.Fields = new List<Field>
            {
                CreateValidField(1, farm.Id, 400m)
            };

            // Act & Assert
            var exception = Record.Exception(() => farm.ValidateAreaForNewField(500m));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateAreaForNewField_WithInsufficientArea_ShouldThrowBusinessException()
        {
            // Arrange
            var farm = CreateValidFarm();
            farm.TotalAreaHectares = 1000m;
            farm.Fields = new List<Field>
            {
                CreateValidField(1, farm.Id, 700m)
            };

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() => farm.ValidateAreaForNewField(400m));
            Assert.Contains("Insufficient area", exception.Message);
        }

        [Fact]
        public void ValidateAreaForNewField_WithExactAvailableArea_ShouldNotThrow()
        {
            // Arrange
            var farm = CreateValidFarm();
            farm.TotalAreaHectares = 1000m;
            farm.Fields = new List<Field>
            {
                CreateValidField(1, farm.Id, 600m)
            };

            // Act & Assert
            var exception = Record.Exception(() => farm.ValidateAreaForNewField(400m));
            Assert.Null(exception);
        }

        #endregion

        #region Deactivate Tests

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var farm = CreateValidFarm();

            // Act
            farm.Deactivate();

            // Assert
            Assert.False(farm.IsActive);
        }

        #endregion

        #region Helper Methods

        private Farm CreateValidFarm()
        {
            return new Farm
            {
                Id = 1,
                ProducerId = "producer123",
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
