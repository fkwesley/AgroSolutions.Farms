using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Xunit;

namespace Tests.UnitTests.Domain.Entities
{
    public class CropSeasonTests
    {
        #region Constructor Tests

        [Fact]
        public void CropSeason_WithValidData_ShouldCreate()
        {
            // Arrange & Act
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(10),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, cropSeason.FieldId);
            Assert.Equal(CropType.Soybean, cropSeason.CropType);
            Assert.Equal(CropSeasonStatus.Planned, cropSeason.Status);
        }

        [Fact]
        public void CropSeason_WithPlantingDateInFarFuture_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var cropSeason = new CropSeason
                {
                    FieldId = 1,
                    CropType = CropType.Soybean,
                    PlantingDate = DateTime.UtcNow.AddYears(2),
                    ExpectedHarvestDate = DateTime.UtcNow.AddYears(2).AddDays(120),
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Planting date cannot be more than 1 year in the future", exception.Message);
        }

        [Fact]
        public void CropSeason_WithExpectedHarvestBeforePlanting_ShouldThrowBusinessException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                var cropSeason = new CropSeason
                {
                    FieldId = 1,
                    CropType = CropType.Soybean,
                    PlantingDate = DateTime.UtcNow.AddDays(100),
                    ExpectedHarvestDate = DateTime.UtcNow.AddDays(50), // Before planting
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Expected harvest date must be after planting date", exception.Message);
        }

        #endregion

        #region StartPlanting Tests

        [Fact]
        public void StartPlanting_WithPlannedStatus_ShouldChangeToActive()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-1); // Past date

            // Act
            cropSeason.StartPlanting();

            // Assert
            Assert.Equal(CropSeasonStatus.Active, cropSeason.Status);
        }

        [Fact]
        public void StartPlanting_WithActiveStatus_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-1);
            cropSeason.StartPlanting(); // Already active

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() => cropSeason.StartPlanting());
            Assert.Contains("Only planned crop seasons can be started", exception.Message);
        }

        [Fact]
        public void StartPlanting_BeforePlantingDate_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(10); // Future date

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() => cropSeason.StartPlanting());
            Assert.Contains("Cannot start planting before the planned planting date", exception.Message);
        }

        #endregion

        #region FinishHarvest Tests

        [Fact]
        public void FinishHarvest_WithActiveStatus_ShouldFinish()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-120);
            cropSeason.StartPlanting();

            var harvestDate = DateTime.UtcNow;

            // Act
            cropSeason.FinishHarvest(harvestDate);

            // Assert
            Assert.Equal(CropSeasonStatus.Finished, cropSeason.Status);
            Assert.Equal(harvestDate, cropSeason.HarvestDate);
        }

        [Fact]
        public void FinishHarvest_WithPlannedStatus_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
                cropSeason.FinishHarvest(DateTime.UtcNow));
            Assert.Contains("Only active crop seasons can be harvested", exception.Message);
        }

        [Fact]
        public void FinishHarvest_WithHarvestDateBeforePlanting_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-120);
            cropSeason.StartPlanting();

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
                cropSeason.FinishHarvest(DateTime.UtcNow.AddDays(-130)));
            Assert.Contains("Harvest date cannot be before planting date", exception.Message);
        }

        [Fact]
        public void FinishHarvest_WithFutureDate_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-120);
            cropSeason.StartPlanting();

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
                cropSeason.FinishHarvest(DateTime.UtcNow.AddDays(10)));
            Assert.Contains("Harvest date cannot be in the future", exception.Message);
        }

        #endregion

        #region Cancel Tests

        [Fact]
        public void Cancel_WithActiveStatus_ShouldCancel()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-1);
            cropSeason.StartPlanting();

            // Act
            cropSeason.Cancel();

            // Assert
            Assert.Equal(CropSeasonStatus.Planned, cropSeason.Status);
            Assert.Null(cropSeason.HarvestDate);
        }

        [Fact]
        public void Cancel_WithFinishedStatus_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-120);
            cropSeason.StartPlanting();
            cropSeason.FinishHarvest(DateTime.UtcNow);

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() => cropSeason.Cancel());
            Assert.Contains("Cannot cancel a finished crop season", exception.Message);
        }

        #endregion

        #region GetCycleDurationInDays Tests

        [Fact]
        public void GetCycleDurationInDays_WithHarvestDate_ShouldReturnActualDuration()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-120);
            cropSeason.ExpectedHarvestDate = DateTime.UtcNow.AddDays(10);
            cropSeason.StartPlanting();
            cropSeason.FinishHarvest(DateTime.UtcNow);

            // Act
            var duration = cropSeason.GetCycleDurationInDays();

            // Assert
            Assert.Equal(120, duration);
        }

        [Fact]
        public void GetCycleDurationInDays_WithoutHarvestDate_ShouldReturnExpectedDuration()
        {
            // Arrange
            var plantingDate = DateTime.UtcNow.AddDays(10);
            var expectedHarvestDate = DateTime.UtcNow.AddDays(130);
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = plantingDate,
                ExpectedHarvestDate = expectedHarvestDate,
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var duration = cropSeason.GetCycleDurationInDays();

            // Assert
            Assert.Equal(120, duration);
        }

        #endregion

        #region IsOverdue Tests

        [Fact]
        public void IsOverdue_WithExpectedHarvestDatePassed_ShouldReturnTrue()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-150);
            cropSeason.ExpectedHarvestDate = DateTime.UtcNow.AddDays(-10);
            cropSeason.StartPlanting();

            // Act
            var isOverdue = cropSeason.IsOverdue();

            // Assert
            Assert.True(isOverdue);
        }

        [Fact]
        public void IsOverdue_WithExpectedHarvestDateInFuture_ShouldReturnFalse()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(10);
            cropSeason.ExpectedHarvestDate = DateTime.UtcNow.AddDays(130);

            // Act
            var isOverdue = cropSeason.IsOverdue();

            // Assert
            Assert.False(isOverdue);
        }

        [Fact]
        public void IsOverdue_WithFinishedStatus_ShouldReturnFalse()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateTime.UtcNow.AddDays(-120);
            cropSeason.ExpectedHarvestDate = DateTime.UtcNow.AddDays(-10);
            cropSeason.StartPlanting();
            cropSeason.FinishHarvest(DateTime.UtcNow);

            // Act
            var isOverdue = cropSeason.IsOverdue();

            // Assert
            Assert.False(isOverdue); // Finished crops are not overdue
        }

        #endregion

        #region Helper Methods

        private CropSeason CreateValidCropSeason()
        {
            return new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(10),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}
