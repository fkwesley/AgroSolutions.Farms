using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories
{
    public interface ICropSeasonRepository
    {
        Task<IEnumerable<CropSeason>> GetAllCropSeasonsAsync();
        Task<CropSeason?> GetCropSeasonByIdAsync(int cropSeasonId);
        Task<IEnumerable<CropSeason>> GetCropSeasonsByFieldIdAsync(int fieldId);
        Task<IEnumerable<CropSeason>> GetCropSeasonsByStatusAsync(CropSeasonStatus status);
        Task<IEnumerable<CropSeason>> GetOverdueCropSeasonsAsync();
        Task<CropSeason> AddCropSeasonAsync(CropSeason cropSeason);
        Task<CropSeason> UpdateCropSeasonAsync(CropSeason cropSeason);
        Task<bool> DeleteCropSeasonAsync(int cropSeasonId);

        /// <summary>
        /// Verifica se há conflito de datas para safras no mesmo campo.
        /// Considera apenas safras Planned e Active.
        /// </summary>
        /// <param name="fieldId">ID do campo</param>
        /// <param name="plantingDate">Data de plantio</param>
        /// <param name="expectedHarvestDate">Data prevista de colheita</param>
        /// <param name="excludeCropSeasonId">ID da safra a ser excluída da verificação (para updates)</param>
        Task<bool> HasDateConflictAsync(int fieldId, DateOnly plantingDate, DateOnly expectedHarvestDate, int? excludeCropSeasonId = null);
    }
}
