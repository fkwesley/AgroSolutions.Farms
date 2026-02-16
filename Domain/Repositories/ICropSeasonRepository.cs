using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories
{
    public interface ICropSeasonRepository
    {
        Task<IEnumerable<CropSeason>> GetAllCropSeasonsAsync();
        Task<CropSeason?> GetCropSeasonByIdAsync(string cropSeasonId);
        Task<IEnumerable<CropSeason>> GetCropSeasonsByFieldIdAsync(string fieldId);
        Task<IEnumerable<CropSeason>> GetCropSeasonsByStatusAsync(CropSeasonStatus status);
        Task<IEnumerable<CropSeason>> GetOverdueCropSeasonsAsync();
        Task<CropSeason> AddCropSeasonAsync(CropSeason cropSeason);
        Task<CropSeason> UpdateCropSeasonAsync(CropSeason cropSeason);
        Task<bool> DeleteCropSeasonAsync(string cropSeasonId);
        Task<bool> CropSeasonExistsAsync(string cropSeasonId);
    }
}
