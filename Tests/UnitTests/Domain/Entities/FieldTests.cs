using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Xunit;

namespace Tests.UnitTests.Domain.Entities
{
    public class FieldTests
    {
        #region Constructor Tests

        [Fact]
        public void Field_WithValidData_ShouldCreate()
        {
            // Arrange & Act
            var field = new Field
            {
                FarmId = 1,
                Name = "Test Field",
                AreaHectares = 100m,
                Latitude = -23.5505m,
                Longitude = -46.6333m,
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, field.FarmId);
            Assert.Equal("Test Field", field.Name);
            Assert.Equal(100m, field.AreaHectares);
            Assert.True(field.IsActive);
        }

        [Fact]
        public void Field_WithZeroArea_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var field = new Field
                {
                    FarmId = 1,
                    Name = "Test Field",
                    AreaHectares = 0m,
                    Latitude = -23.5505m,
                    Longitude = -46.6333m,
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Field area must be greater than zero", exception.Message);
        }

        [Fact]
        public void Field_WithNegativeArea_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var field = new Field
                {
                    FarmId = 1,
                    Name = "Test Field",
                    AreaHectares = -50m,
                    Latitude = -23.5505m,
                    Longitude = -46.6333m,
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Field area must be greater than zero", exception.Message);
        }

        [Fact]
        public void Field_WithInvalidLatitude_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var field = new Field
                {
                    FarmId = 1,
                    Name = "Test Field",
                    AreaHectares = 100m,
                    Latitude = 95m, // Invalid: > 90
                    Longitude = -46.6333m,
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Latitude must be between -90 and 90 degrees", exception.Message);
        }

        [Fact]
        public void Field_WithInvalidLongitude_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var field = new Field
                {
                    FarmId = 1,
                    Name = "Test Field",
                    AreaHectares = 100m,
                    Latitude = -23.5505m,
                    Longitude = 200m, // Invalid: > 180
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Longitude must be between -180 and 180 degrees", exception.Message);
        }

        #endregion

        #region ValidateAreaAgainstFarm Tests

        [Fact]
        public void ValidateAreaAgainstFarm_WithValidArea_ShouldNotThrow()
        {
            // Arrange
            var field = CreateValidField();
            field.AreaHectares = 100m;

            // Act & Assert
            var exception = Record.Exception(() => field.ValidateAreaAgainstFarm(1000m, 500m));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateAreaAgainstFarm_ExceedingFarmArea_ShouldThrowBusinessException()
        {
            // Arrange
            var field = CreateValidField();
            field.AreaHectares = 600m;

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() => 
                field.ValidateAreaAgainstFarm(1000m, 500m)); // 600 + 500 > 1000
            Assert.Contains("exceeds farm total area", exception.Message);
        }

        [Fact]
        public void ValidateAreaAgainstFarm_WithExactFarmArea_ShouldNotThrow()
        {
            // Arrange
            var field = CreateValidField();
            field.AreaHectares = 400m;

            // Act & Assert
            var exception = Record.Exception(() => field.ValidateAreaAgainstFarm(1000m, 600m)); // 400 + 600 = 1000
            Assert.Null(exception);
        }

        #endregion

        #region CropSeasons Tests

        [Fact]
        public void Field_CropSeasons_ShouldBeInitializedAsEmptyList()
        {
            // Arrange & Act
            var field = CreateValidField();

            // Assert
            Assert.NotNull(field.CropSeasons);
            Assert.Empty(field.CropSeasons);
        }

        [Fact]
        public void Field_WithActiveCropSeasons_ShouldContainThem()
        {
            // Arrange
            var field = CreateValidField();
            field.CropSeasons = new List<CropSeason>
            {
                CreateCropSeason(1, CropSeasonStatus.Active),
                CreateCropSeason(2, CropSeasonStatus.Planned)
            };

            // Act
            var activeCropSeasons = field.CropSeasons.Where(cs => cs.Status == CropSeasonStatus.Active).ToList();

            // Assert
            Assert.Single(activeCropSeasons);
            Assert.Equal(CropSeasonStatus.Active, activeCropSeasons[0].Status);
        }

        [Fact]
        public void Field_WithNoCropSeasons_ShouldHaveEmptyCollection()
        {
            // Arrange
            var field = CreateValidField();

            // Act
            var activeCropSeasons = field.CropSeasons.Where(cs => cs.Status == CropSeasonStatus.Active).ToList();

            // Assert
            Assert.Empty(activeCropSeasons);
        }

        #endregion

        #region Deactivate Tests

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var field = CreateValidField();

            // Act
            field.Deactivate();

            // Assert
            Assert.False(field.IsActive);
        }

        #endregion

        #region Helper Methods

        private Field CreateValidField()
        {
            return new Field
            {
                Id = 1,
                FarmId = 1,
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

        private CropSeason CreateCropSeason(int id, CropSeasonStatus status)
        {
            var cropSeason = new CropSeason
            {
                Id = id,
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                Status = status,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            if (status == CropSeasonStatus.Finished)
            {
                cropSeason.HarvestDate = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            return cropSeason;
        }

        #endregion
    }
}
