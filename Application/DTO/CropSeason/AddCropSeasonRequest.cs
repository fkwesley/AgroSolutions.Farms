using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTO.CropSeason
{
    public class AddCropSeasonRequest
    {
        [Required]
        [MaxLength(20)]
        public required int FieldId { get; set; }

        [Required]
        public required CropType CropType { get; set; }

        [Required]
        public required DateTime PlantingDate { get; set; }

        [Required]
        public required DateTime ExpectedHarvestDate { get; set; }
        
        [JsonIgnore]
        public required string CreatedBy { get; set; }
    }
}
