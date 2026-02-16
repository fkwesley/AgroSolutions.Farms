using Application.DTO.Common;
using Domain.Enums;

namespace Application.DTO.CropSeason
{
    public class CropSeasonResponse : IHateoasResource
    {
        public required int Id { get; set; }
        public required int FieldId { get; set; }
        public required string CropType { get; set; }
        public required DateTime PlantingDate { get; set; }
        public required DateTime ExpectedHarvestDate { get; set; }
        public DateTime? HarvestDate { get; set; }
        public required string Status { get; set; }
        public int CycleDurationDays { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<Link> Links { get; set; } = new();
    }
}
