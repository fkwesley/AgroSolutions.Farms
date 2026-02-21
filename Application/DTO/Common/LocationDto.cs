using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Common
{
    public class LocationDto
    {
        [Required]
        [MaxLength(100)]
        public required string City { get; set; }

        [Required]
        [MaxLength(100)]
        public required string State { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Country { get; set; }
    }
}
