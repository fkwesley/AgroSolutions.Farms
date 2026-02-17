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
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
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
                    PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                    ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2).AddDays(120)),
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
                    PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100)),
                    ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(50)), // Before planting
                    CreatedBy = "user123",
                    CreatedAt = DateTime.UtcNow
                };
            });

            Assert.Contains("Expected harvest date must be after planting date", exception.Message);
        }

        #endregion

        #region HarvestDate Setter Tests

        [Fact]
        public void HarvestDate_WithFutureDate_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-100)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            };

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                cropSeason.HarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)); // Future date
            });

            Assert.Contains("Harvest date cannot be in the future", exception.Message);
        }

        [Fact]
        public void HarvestDate_WithDateBeforePlanting_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-100)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            };

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
            {
                cropSeason.HarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-110)); // Before planting
            });

            Assert.Contains("Harvest date cannot be before planting date", exception.Message);
        }

        [Fact]
        public void HarvestDate_WithValidPastDate_ShouldSetAndChangeStatusToFinished()
        {
            // Arrange
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-100)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            };

            var harvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

            // Act
            cropSeason.HarvestDate = harvestDate;

            // Assert
            Assert.Equal(harvestDate, cropSeason.HarvestDate);
            Assert.Equal(CropSeasonStatus.Finished, cropSeason.Status);
        }

        [Fact]
        public void HarvestDate_WithTodayDate_ShouldSetAndChangeStatusToFinished()
        {
            // Arrange
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-100)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            };

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            cropSeason.HarvestDate = today;

            // Assert
            Assert.Equal(today, cropSeason.HarvestDate);
            Assert.Equal(CropSeasonStatus.Finished, cropSeason.Status);
        }

        [Fact]
        public void CropSeason_WithHarvestDateInConstructor_ShouldSetStatusToFinished()
        {
            // Arrange & Act
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                HarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), // Past date
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };

            // Assert
            Assert.Equal(CropSeasonStatus.Finished, cropSeason.Status);
            Assert.NotNull(cropSeason.HarvestDate);
        }

        #endregion

        #region StartPlanting Tests

        [Fact]
        public void StartPlanting_WithPlannedStatus_ShouldChangeToActive()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)); // Amanhã = Planned

            // Act
            cropSeason.StartPlanting();

            // Assert
            Assert.Equal(CropSeasonStatus.Active, cropSeason.Status);
        }

        [Fact]
        public void StartPlanting_WithActiveStatus_ShouldThrowBusinessException()
        {
            // Arrange - cria safra com plantio no passado = Active automaticamente
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(110)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            // Act & Assert - já está Active, não pode chamar StartPlanting
            var exception = Assert.Throws<BusinessException>(() => cropSeason.StartPlanting());
            Assert.Contains("Only planned crop seasons can be started", exception.Message);
        }

        [Fact]
        public void StartPlanting_BeforePlantingDate_ShouldThrowBusinessException()
        {
            // Arrange
            var cropSeason = CreateValidCropSeason();
            cropSeason.PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)); // 10 dias no futuro

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() => cropSeason.StartPlanting());
            Assert.Contains("Cannot start planting more than 1 day before", exception.Message);
        }

        #endregion

        #region FinishHarvest Tests

        [Fact]
        public void FinishHarvest_WithActiveStatus_ShouldFinish()
        {
            // Arrange - cria safra já ativa
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };

            var harvestDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            cropSeason.FinishHarvest(harvestDate);

            // Assert
            Assert.Equal(CropSeasonStatus.Finished, cropSeason.Status);
            Assert.Equal(harvestDate, cropSeason.HarvestDate);
        }

        [Fact]
        public void FinishHarvest_WithPlannedStatus_ShouldThrowBusinessException()
        {
            // Arrange - safra planejada (PlantingDate no futuro)
            var cropSeason = CreateValidCropSeason();

            // Act & Assert - tentar colher antes do plantio
            var exception = Assert.Throws<BusinessException>(() =>
                cropSeason.FinishHarvest(DateOnly.FromDateTime(DateTime.UtcNow)));
            Assert.Contains("Harvest date cannot be before planting date", exception.Message);
        }

        [Fact]
        public void FinishHarvest_WithHarvestDateBeforePlanting_ShouldThrowBusinessException()
        {
            // Arrange - cria safra já ativa (plantio no passado)
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
                cropSeason.FinishHarvest(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-130))));
            Assert.Contains("Harvest date cannot be before planting date", exception.Message);
        }

        [Fact]
        public void FinishHarvest_WithFutureDate_ShouldThrowBusinessException()
        {
            // Arrange - cria safra já ativa (plantio no passado)
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() =>
                cropSeason.FinishHarvest(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))));
            Assert.Contains("Harvest date cannot be in the future", exception.Message);
        }

        #endregion

        #region Cancel Tests

        [Fact]
        public void Cancel_WithActiveStatus_ShouldCancel()
        {
            // Arrange - cria safra já ativa (plantio no passado)
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(110)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            // Act
            cropSeason.Cancel();

            // Assert
            Assert.Equal(CropSeasonStatus.Canceled, cropSeason.Status);
            Assert.Null(cropSeason.HarvestDate);
        }

        [Fact]
        public void Cancel_WithFinishedStatus_ShouldThrowBusinessException()
        {
            // Arrange - cria safra finalizada
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };
            cropSeason.FinishHarvest(DateOnly.FromDateTime(DateTime.UtcNow));

            // Act & Assert
            var exception = Assert.Throws<BusinessException>(() => cropSeason.Cancel());
            Assert.Contains("Cannot cancel a finished crop season", exception.Message);
        }

        #endregion

        #region GetCycleDurationInDays Tests

        [Fact]
        public void GetCycleDurationInDays_WithHarvestDate_ShouldReturnActualDuration()
        {
            // Arrange - cria safra finalizada
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };
            cropSeason.FinishHarvest(DateOnly.FromDateTime(DateTime.UtcNow));

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
                PlantingDate = DateOnly.FromDateTime(plantingDate),
                ExpectedHarvestDate = DateOnly.FromDateTime(expectedHarvestDate),
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
            // Arrange - cria safra ativa atrasada
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-150)), // Passado = Active
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), // Atrasada
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-150)
            };

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
            cropSeason.PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
            cropSeason.ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130));

            // Act
            var isOverdue = cropSeason.IsOverdue();

            // Assert
            Assert.False(isOverdue);
        }

        [Fact]
        public void IsOverdue_WithFinishedStatus_ShouldReturnFalse()
        {
            // Arrange - cria safra finalizada
            var cropSeason = new CropSeason
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            };
            cropSeason.FinishHarvest(DateOnly.FromDateTime(DateTime.UtcNow));

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
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}
