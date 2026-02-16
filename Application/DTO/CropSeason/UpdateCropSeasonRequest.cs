using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTO.CropSeason
{
    public class UpdateCropSeasonRequest
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        public required DateTime PlantingDate { get; set; }

        [Required]
        public required DateTime ExpectedHarvestDate { get; set; }



        [JsonIgnore]
        public required string UpdatedBy { get; set; }
    }
}
