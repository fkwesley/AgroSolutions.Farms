using Application.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTO.Farm
{
    public class AddFarmRequest
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [JsonIgnore]
        public string ProducerId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total area must be greater than zero.")]
        public required decimal TotalAreaHectares { get; set; }

        [Required]
        public required LocationDto Location { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
