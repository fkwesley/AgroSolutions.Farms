using Application.DTO.Field;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mappings
{
    public static class FieldMappingExtensions
    {
        /// <summary>
        /// Maps a AddFieldRequest to a Field entity.
        /// </summary>
        public static Field ToEntity(this AddFieldRequest request)
        {
            return new Field
            {
                Id = 0,
                FarmId = request.FarmId,
                Name = request.Name.Trim(),
                AreaHectares = request.AreaHectares,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsActive = request.IsActive,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Maps a UpdateFieldRequest to a Field entity.
        /// </summary>
        public static Field ToEntity(this UpdateFieldRequest request)
        {
            return new Field
            {
                Id = request.Id,
                FarmId = request.FarmId,
                Name = request.Name.Trim(),
                AreaHectares = request.AreaHectares,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsActive = request.IsActive,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = String.Empty // This will be set when the entity is retrieved from the database for update
            };
        }

        /// <summary>
        /// Maps a Field entity to a FieldResponse.
        /// </summary>
        public static FieldResponse ToResponse(this Field entity)
        {
            return new FieldResponse
            {
                Id = entity.Id,
                FarmId = entity.FarmId,
                Name = entity.Name,
                AreaHectares = entity.AreaHectares,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                IsActive = entity.IsActive,
                TotalCropSeasons = entity.CropSeasons.Count,
                ActiveCropSeasons = entity.CropSeasons.Count(cs => cs.Status == CropSeasonStatus.Active),
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
