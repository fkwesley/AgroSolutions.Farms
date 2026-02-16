using Application.DTO.Common;

namespace Application.DTO.Farm
{
    public class FarmResponse : IHateoasResource
    {
        public required int FarmId { get; set; }
        public required string FarmName { get; set; }
        public required string ProducerId { get; set; }
        public required decimal TotalAreaHectares { get; set; }
        public bool IsActive { get; set; }
        public required LocationDto Location { get; set; }
        public decimal UsedAreaHectares { get; set; }
        public decimal AvailableAreaHectares { get; set; }
        public int TotalFields { get; set; }
        public required string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<Link> Links { get; set; } = new();
    }
}
