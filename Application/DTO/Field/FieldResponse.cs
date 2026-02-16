using Application.DTO.Common;

namespace Application.DTO.Field
{
    public class FieldResponse : IHateoasResource
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        public required string Name { get; set; }
        public required decimal AreaHectares { get; set; }
        public required decimal Latitude { get; set; }
        public required decimal Longitude { get; set; }
        public bool IsActive { get; set; }
        public int TotalCropSeasons { get; set; }
        public int ActiveCropSeasons { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<Link> Links { get; set; } = new();
    }
}
