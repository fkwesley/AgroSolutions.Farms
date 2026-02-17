using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTO.CropSeason
{
    public class AddCropSeasonRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "FieldId must be a positive number")]
        public required int FieldId { get; set; }

        [Required]
        [EnumDataType(typeof(CropType), ErrorMessage = "Invalid crop type")]
        public required CropType CropType { get; set; }

        [Required]
        public required DateOnly PlantingDate { get; set; }

        public DateOnly? HarvestDate { get; set; }

        [Required]
        public required DateOnly ExpectedHarvestDate { get; set; }

        [JsonIgnore]
        public string CreatedBy { get; set; } = string.Empty;
    }
}
