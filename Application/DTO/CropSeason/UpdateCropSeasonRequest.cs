using System.ComponentModel.DataAnnotations;

namespace Application.DTO.CropSeason
{
    public class UpdateCropSeasonRequest
    {
        [Required]
        [MaxLength(20)]
        public required string Id { get; set; }

        [Required]
        public required DateTime PlantingDate { get; set; }

        [Required]
        public required DateTime ExpectedHarvestDate { get; set; }
    }
}
