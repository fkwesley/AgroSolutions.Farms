using Application.DTO.CropSeason;
using Domain.Enums;

namespace Application.Interfaces
{
    // #SOLID - Interface Segregation Principle (ISP)
    // Esta interface define apenas os métodos relacionados a operações de safras.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Esta interface permite que camadas superiores (API) dependam de abstração,
    // não da implementação concreta (CropSeasonService).
    public interface ICropSeasonService
    {
        Task<IEnumerable<CropSeasonResponse>> GetAllCropSeasonsAsync();
        Task<CropSeasonResponse> GetCropSeasonByIdAsync(string cropSeasonId);
        Task<IEnumerable<CropSeasonResponse>> GetCropSeasonsByFieldIdAsync(string fieldId);
        Task<IEnumerable<CropSeasonResponse>> GetCropSeasonsByStatusAsync(CropSeasonStatus status);
        Task<IEnumerable<CropSeasonResponse>> GetOverdueCropSeasonsAsync();
        Task<CropSeasonResponse> AddCropSeasonAsync(AddCropSeasonRequest request);
        Task<CropSeasonResponse> UpdateCropSeasonAsync(UpdateCropSeasonRequest request);
        Task<CropSeasonResponse> StartPlantingAsync(string cropSeasonId);
        Task<CropSeasonResponse> FinishHarvestAsync(string cropSeasonId, DateTime harvestDate);
        Task<CropSeasonResponse> CancelCropSeasonAsync(string cropSeasonId);
        Task<bool> DeleteCropSeasonAsync(string cropSeasonId);
    }
}
