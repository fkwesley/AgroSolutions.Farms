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
        Task<CropSeasonResponse> GetCropSeasonByIdAsync(int cropSeasonId);
        Task<IEnumerable<CropSeasonResponse>> GetCropSeasonsByFieldIdAsync(int fieldId);
        Task<IEnumerable<CropSeasonResponse>> GetCropSeasonsByStatusAsync(CropSeasonStatus status);
        Task<IEnumerable<CropSeasonResponse>> GetOverdueCropSeasonsAsync();
        Task<CropSeasonResponse> AddCropSeasonAsync(AddCropSeasonRequest request);
        Task<CropSeasonResponse> UpdateCropSeasonAsync(UpdateCropSeasonRequest request);
        Task<CropSeasonResponse> StartPlantingAsync(int cropSeasonId, string updatedBy);
        Task<CropSeasonResponse> FinishHarvestAsync(int cropSeasonId, DateTime harvestDate, string updatedBy);
        Task<CropSeasonResponse> CancelCropSeasonAsync(int cropSeasonId, string updatedBy);
        Task<bool> DeleteCropSeasonAsync(int cropSeasonId);
    }
}
