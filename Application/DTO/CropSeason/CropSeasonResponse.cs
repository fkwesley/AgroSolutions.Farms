using Application.DTO.Common;
using Domain.Enums;

namespace Application.DTO.CropSeason
{
    public class CropSeasonResponse : IHateoasResource
    {
        public required int Id { get; set; }
        public required int FieldId { get; set; }
        public required CropType CropType { get; set; }
        public required DateOnly PlantingDate { get; set; }
        public required DateOnly ExpectedHarvestDate { get; set; }
        public DateOnly? HarvestDate { get; set; }
        public required string Status { get; set; }
        public int CycleDurationDays { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<Link> Links { get; set; } = new();
    }
}
