using System.ComponentModel.DataAnnotations;

namespace Application.DTO.CropSeason
{
    public class FinishHarvestRequest
    {
        [Required]
        public required DateOnly HarvestDate { get; set; }
    }
}
