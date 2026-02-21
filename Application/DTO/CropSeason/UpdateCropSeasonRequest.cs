using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTO.CropSeason
{
    public class UpdateCropSeasonRequest
    {
        [JsonIgnore]
        public int Id { get; set; }

        [EnumDataType(typeof(CropType), ErrorMessage = "Invalid crop type")]
        public CropType? CropType { get; set; }

        public DateOnly? ExpectedHarvestDate { get; set; }

        [JsonIgnore]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
