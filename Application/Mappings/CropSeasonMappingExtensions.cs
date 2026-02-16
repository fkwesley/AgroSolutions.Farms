using Application.DTO.CropSeason;
using Domain.Entities;

namespace Application.Mappings
{
    public static class CropSeasonMappingExtensions
    {
        /// <summary>
        /// Maps a AddCropSeasonRequest to a CropSeason entity.
        /// </summary>
        public static CropSeason ToEntity(this AddCropSeasonRequest request)
        {
            return new CropSeason
            {
                FieldId = request.FieldId,
                CropType = request.CropType,
                PlantingDate = request.PlantingDate,
                ExpectedHarvestDate = request.ExpectedHarvestDate,
                Status = Domain.Enums.CropSeasonStatus.Planned,
                CreatedBy = request.CreatedBy
            };
        }

        /// <summary>
        /// Maps a UpdateCropSeasonRequest to update a CropSeason entity.
        /// </summary>
        public static void UpdateFromRequest(this CropSeason entity, UpdateCropSeasonRequest request)
        {
            entity.PlantingDate = request.PlantingDate;
            entity.ExpectedHarvestDate = request.ExpectedHarvestDate;
        }

        /// <summary>
        /// Maps a CropSeason entity to a CropSeasonResponse.
        /// </summary>
        public static CropSeasonResponse ToResponse(this CropSeason entity)
        {
            return new CropSeasonResponse
            {
                Id = entity.Id,
                FieldId = entity.FieldId,
                CropType = entity.CropType.ToString(),
                PlantingDate = entity.PlantingDate,
                ExpectedHarvestDate = entity.ExpectedHarvestDate,
                HarvestDate = entity.HarvestDate,
                Status = entity.Status.ToString(),
                CycleDurationDays = entity.GetCycleDurationInDays(),
                IsOverdue = entity.IsOverdue(),
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
