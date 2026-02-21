using Application.DTO.Common;
using Application.DTO.Farm;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Mappings
{
    public static class FarmMappingExtensions
    {
        /// <summary>
        /// Maps a AddFarmRequest to a Farm entity.
        /// </summary>
        public static Farm ToEntity(this AddFarmRequest request)
        {
            return new Farm
            {
                Id = 0,
                Name = request.Name.Trim(),
                ProducerId = request.ProducerId.ToUpper(),
                TotalAreaHectares = request.TotalAreaHectares,
                IsActive = request.IsActive,
                Location = new Location(
                    request.Location.City.Trim(),
                    request.Location.State.Trim(),
                    request.Location.Country.Trim()
                ),
                CreatedBy = request.ProducerId.ToUpper(),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Maps a UpdateFarmRequest to a Farm entity.
        /// </summary>
        public static Farm ToEntity(this UpdateFarmRequest request)
        {
            return new Farm
            {
                Id = request.FarmId,
                Name = request.FarmName.Trim(),
                TotalAreaHectares = request.TotalAreaHectares,
                IsActive = request.IsActive,
                Location = new Location(
                    request.Location.City.Trim(),
                    request.Location.State.Trim(),
                    request.Location.Country.Trim()
                ),
                ProducerId = string.Empty, // Será preenchido no service
                CreatedBy = string.Empty, // Será preenchido no service
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Maps a Farm entity to a FarmResponse.
        /// </summary>
        public static FarmResponse ToResponse(this Farm entity)
        {
            return new FarmResponse
            {
                FarmId = entity.Id,
                FarmName = entity.Name,
                ProducerId = entity.ProducerId,
                TotalAreaHectares = entity.TotalAreaHectares,
                IsActive = entity.IsActive,
                Location = new LocationDto
                {
                    City = entity.Location.City,
                    State = entity.Location.State,
                    Country = entity.Location.Country
                },
                UsedAreaHectares = entity.GetTotalFieldsArea(),
                AvailableAreaHectares = entity.GetAvailableArea(),
                TotalFields = entity.Fields.Count,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
