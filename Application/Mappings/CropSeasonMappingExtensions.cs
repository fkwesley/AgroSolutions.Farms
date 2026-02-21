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
                HarvestDate = request.HarvestDate,
                CreatedBy = request.CreatedBy
            };
        }

        /// <summary>
        /// Maps a UpdateCropSeasonRequest to update a CropSeason entity (partial update).
        /// </summary>
        public static void UpdateFromRequest(this CropSeason entity, UpdateCropSeasonRequest request)
        {
            // Atualiza apenas os campos que foram fornecidos (não nulos)
            if (request.CropType.HasValue)
                entity.CropType = request.CropType.Value;

            if (request.ExpectedHarvestDate.HasValue)
                entity.ExpectedHarvestDate = request.ExpectedHarvestDate.Value;
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
                CropType = entity.CropType,
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
