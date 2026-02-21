using Application.DTO.Farm;

namespace Application.Interfaces
{
    // #SOLID - Interface Segregation Principle (ISP)
    // Esta interface define apenas os métodos relacionados a operações de fazendas.

    // #SOLID - Dependency Inversion Principle (DIP)
    // Esta interface permite que camadas superiores (API) dependam de abstração,
    // não da implementação concreta (FarmService).
    public interface IFarmService
    {
        Task<IEnumerable<FarmResponse>> GetAllFarmsAsync();
        Task<FarmResponse> GetFarmByIdAsync(int farmId);
        Task<FarmResponse> AddFarmAsync(AddFarmRequest request);
        Task<FarmResponse> UpdateFarmAsync(UpdateFarmRequest request);
        Task<bool> DeleteFarmAsync(int farmId);
    }
}
