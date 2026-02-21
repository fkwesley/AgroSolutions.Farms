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
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
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
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130)),
                Status = CropSeasonStatus.Planned,
                CreatedBy = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var newExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(135));
            var updateRequest = new UpdateCropSeasonRequest
            {
                Id = 1,
                CropType = CropType.Corn, // Mudando o tipo de cultura
                ExpectedHarvestDate = newExpectedHarvestDate,
                UpdatedBy = "user456"
            };

            // Act
            existingEntity.UpdateFromRequest(updateRequest);

            // Assert
            Assert.Equal(1, existingEntity.Id); // ID não muda
            Assert.Equal(1, existingEntity.FieldId); // FieldId não muda
            Assert.Equal(CropType.Corn, existingEntity.CropType); // CropType atualizado
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), existingEntity.PlantingDate); // PlantingDate não muda
            Assert.Equal(newExpectedHarvestDate, existingEntity.ExpectedHarvestDate); // ExpectedHarvestDate atualizado
            Assert.Equal(CropSeasonStatus.Planned, existingEntity.Status); // Status não muda
            Assert.Equal("user123", existingEntity.CreatedBy); // CreatedBy não muda
        }

        #endregion

        #region ToResponse Tests

        [Fact]
        public void ToResponse_ShouldMapAllProperties()
        {
            // Arrange
            var plantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120));
            var expectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var harvestDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var createdAt = DateTime.UtcNow.AddDays(-130);
            var updatedAt = DateTime.UtcNow;

            var entity = new CropSeason
            {
                Id = 1,
                FieldId = 2,
                CropType = CropType.Soybean,
                PlantingDate = plantingDate,
                ExpectedHarvestDate = expectedHarvestDate,
                HarvestDate = harvestDate,
                Status = CropSeasonStatus.Finished,
                CreatedBy = "user123",
                CreatedAt = createdAt,
                UpdatedBy = "user456",
                UpdatedAt = updatedAt
            };

            // Act
            var response = entity.ToResponse();

            // Assert
            Assert.NotNull(response);
            Assert.Equal(1, response.Id);
            Assert.Equal(2, response.FieldId);
            Assert.Equal(CropType.Soybean, response.CropType);
            Assert.Equal(plantingDate, response.PlantingDate);
            Assert.Equal(expectedHarvestDate, response.ExpectedHarvestDate);
            Assert.Equal(harvestDate, response.HarvestDate);
            Assert.Equal(CropSeasonStatus.Finished.ToString(), response.Status);
            Assert.Equal(120, response.CycleDurationDays);
            Assert.False(response.IsOverdue); // Finished crops are not overdue
            Assert.Equal(createdAt, response.CreatedAt);
            Assert.Equal(updatedAt, response.UpdatedAt);
        }

        [Fact]
        public void ToResponse_WithoutHarvestDate_ShouldCalculateExpectedDuration()
        {
            // Arrange
            var plantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
            var expectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(130));

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
                PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-150)),
                ExpectedHarvestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), // Past date
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
