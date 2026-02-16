using Domain.Entities;

namespace Domain.Repositories
{
    public interface IFarmRepository
    {
        Task<IEnumerable<Farm>> GetAllFarmsAsync();
        Task<Farm?> GetFarmByIdAsync(int farmId);
        Task<IEnumerable<Farm>> GetFarmsByProducerIdAsync(string producerId);
        Task<Farm> AddFarmAsync(Farm farm);
        Task<Farm> UpdateFarmAsync(Farm farm);
        Task<bool> DeleteFarmAsync(int farmId);
        Task<bool> FarmExistsAsync(string farmName);
    }
}
