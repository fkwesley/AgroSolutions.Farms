using Application.DTO.CropSeason;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Tests.UnitTests.Application.Mappings
{
    public class CropSeasonMappingExtensionsTests
    {
        #region ToEntity Tests

        [Fact]
        public void ToEntity_FromAddRequest_ShouldMapCorrectly()
        {
            // Arrange
            var request = new AddCropSeasonRequest
            {
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(10),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                CreatedBy = "user123"
            };

            // Act
            var entity = request.ToEntity();

            // Assert
            Assert.NotNull(entity);
            Assert.Equal(0, entity.Id); // ID não é setado no mapping
            Assert.Equal(1, entity.FieldId);
            Assert.Equal(CropType.Soybean, entity.CropType);
            Assert.Equal(request.PlantingDate, entity.PlantingDate);
            Assert.Equal(request.ExpectedHarvestDate, entity.ExpectedHarvestDate);
            Assert.Equal(CropSeasonStatus.Planned, entity.Status);
        }

        #endregion

        #region UpdateFromRequest Tests

        [Fact]
        public void UpdateFromRequest_ShouldUpdateOnlyAllowedProperties()
        {
            // Arrange
            var existingEntity = new CropSeason
            {
                Id = 1,
                FieldId = 1,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(10),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(130),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var updateRequest = new UpdateCropSeasonRequest
            {
                Id = 1,
                PlantingDate = DateTime.UtcNow.AddDays(15),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(135),
                UpdatedBy = "user456"
            };

            // Act
            existingEntity.UpdateFromRequest(updateRequest);

            // Assert
            Assert.Equal(1, existingEntity.Id); // ID não muda
            Assert.Equal(1, existingEntity.FieldId); // FieldId não muda
            Assert.Equal(CropType.Soybean, existingEntity.CropType); // CropType não muda
            Assert.Equal(updateRequest.PlantingDate, existingEntity.PlantingDate); // Atualizado
            Assert.Equal(updateRequest.ExpectedHarvestDate, existingEntity.ExpectedHarvestDate); // Atualizado
            Assert.Equal(CropSeasonStatus.Planned, existingEntity.Status); // Status não muda
            Assert.Equal("user123", existingEntity.CreatedBy); // CreatedBy não muda
        }

        #endregion

        #region ToResponse Tests

        [Fact]
        public void ToResponse_ShouldMapAllProperties()
        {
            // Arrange
            var plantingDate = DateTime.UtcNow.AddDays(-120);
            var harvestDate = DateTime.UtcNow;
            var entity = new CropSeason
            {
                Id = 1,
                FieldId = 2,
                CropType = CropType.Soybean,
                PlantingDate = plantingDate,
                ExpectedHarvestDate = plantingDate.AddDays(120),
                HarvestDate = harvestDate,
                Status = CropSeasonStatus.Finished,
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-130),
                UpdatedBy = "user456",
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var response = entity.ToResponse();

            // Assert
            Assert.NotNull(response);
            Assert.Equal(1, response.Id);
            Assert.Equal(2, response.FieldId);
            Assert.Equal(CropType.Soybean.ToString(), response.CropType);
            Assert.Equal(plantingDate, response.PlantingDate);
            Assert.Equal(harvestDate, response.HarvestDate);
            Assert.Equal(CropSeasonStatus.Finished.ToString(), response.Status);
            Assert.Equal(120, response.CycleDurationDays);
            Assert.False(response.IsOverdue); // Finished crops are not overdue
            Assert.NotNull(response.CreatedAt);
            Assert.NotNull(response.UpdatedAt);
        }

        [Fact]
        public void ToResponse_WithoutHarvestDate_ShouldCalculateExpectedDuration()
        {
            // Arrange
            var plantingDate = DateTime.UtcNow.AddDays(10);
            var expectedHarvestDate = plantingDate.AddDays(120);
            var entity = new CropSeason
            {
                Id = 1,
                FieldId = 2,
                CropType = CropType.Corn,
                PlantingDate = plantingDate,
                ExpectedHarvestDate = expectedHarvestDate,
                Status = CropSeasonStatus.Planned,
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var response = entity.ToResponse();

            // Assert
            Assert.NotNull(response);
            Assert.Equal(120, response.CycleDurationDays);
            Assert.Null(response.HarvestDate);
            Assert.False(response.IsOverdue);
        }

        [Fact]
        public void ToResponse_WithOverdueCropSeason_ShouldSetIsOverdueTrue()
        {
            // Arrange
            var entity = new CropSeason
            {
                Id = 1,
                FieldId = 2,
                CropType = CropType.Soybean,
                PlantingDate = DateTime.UtcNow.AddDays(-150),
                ExpectedHarvestDate = DateTime.UtcNow.AddDays(-10), // Past date
                Status = CropSeasonStatus.Active,
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-150)
            };

            // Act
            var response = entity.ToResponse();

            // Assert
            Assert.True(response.IsOverdue);
        }

        #endregion
    }
}
