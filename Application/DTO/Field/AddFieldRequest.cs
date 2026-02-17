using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTO.Field
{
    public class AddFieldRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "FarmId must be a positive number")]
        public int FarmId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Area must be greater than zero.")]
        public required decimal AreaHectares { get; set; }

        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees.")]
        public required decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees.")]
        public required decimal Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public string CreatedBy { get; set; } = string.Empty;
    }
}
