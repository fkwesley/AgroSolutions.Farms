using Application.DTO.Farm;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    // #SOLID - Single Responsibility Principle (SRP)
    // FarmService é responsável apenas pela lógica de negócio de fazendas.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Depende de abstrações (IFarmRepository) e não de implementações concretas.
    public class FarmService : IFarmService
    {
        private readonly IFarmRepository _farmRepository;
        private readonly ILogger<FarmService> _logger;

        public FarmService(
            IFarmRepository farmRepository,
            ILogger<FarmService> logger)
        {
            _farmRepository = farmRepository ?? throw new ArgumentNullException(nameof(farmRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<FarmResponse>> GetAllFarmsAsync()
        {
            var farms = await _farmRepository.GetAllFarmsAsync();
            _logger.LogInformation("Retrieved {FarmCount} farms", farms.Count());
            return farms.Select(f => f.ToResponse()).ToList();
        }

        public async Task<FarmResponse> GetFarmByIdAsync(int farmId)
        {
            var farm = await _farmRepository.GetFarmByIdAsync(farmId);

            if (farm == null)
                throw new KeyNotFoundException($"Farm with ID {farmId} not found.");

            return farm.ToResponse();
        }

        public async Task<FarmResponse> AddFarmAsync(AddFarmRequest request)
        {
            // Valida se o FarmId já existe
            if (await _farmRepository.FarmExistsAsync(request.Name))
                throw new ValidationException($"Farm with name {request.Name} already exists.");

            var farmEntity = request.ToEntity();
            farmEntity.SetCreatedAudit(request.ProducerId);

            var addedFarm = await _farmRepository.AddFarmAsync(farmEntity);

            _logger.LogInformation("Farm {FarmId} added successfully by producer {ProducerId}", addedFarm.Id, request.ProducerId);
            return addedFarm.ToResponse();
        }

        public async Task<FarmResponse> UpdateFarmAsync(UpdateFarmRequest request)
        {
            var existingFarm = await _farmRepository.GetFarmByIdAsync(request.FarmId);

            if (existingFarm == null)
                throw new KeyNotFoundException($"Farm with ID {request.FarmId} not found.");

            // Valida se a nova área total é suficiente para os campos existentes
            var usedArea = existingFarm.GetTotalFieldsArea();

            if (request.TotalAreaHectares < usedArea)
                throw new ValidationException($"Cannot reduce farm area to {request.TotalAreaHectares} ha. Current fields occupy {usedArea} ha.");

            var farmEntity = request.ToEntity();
            farmEntity.ProducerId = existingFarm.ProducerId;
            farmEntity.CreatedBy = existingFarm.CreatedBy;
            farmEntity.CreatedAt = existingFarm.CreatedAt;
            farmEntity.SetUpdatedAudit(request.UpdatedBy!); // UpdatedBy foi setado no controller
            farmEntity.Fields = existingFarm.Fields;

            var updatedFarm = await _farmRepository.UpdateFarmAsync(farmEntity);

            _logger.LogInformation("Farm {FarmId} updated successfully by {UserId}", request.FarmId, request.UpdatedBy);
            return updatedFarm.ToResponse();
        }

        public async Task<bool> DeleteFarmAsync(int farmId)
        {
            var farm = await _farmRepository.GetFarmByIdAsync(farmId);

            if (farm == null)
                throw new KeyNotFoundException($"Farm with ID {farmId} not found.");

            // Validação: não permitir deletar fazenda com campos ativos
            if (farm.Fields.Any(f => f.IsActive))
                throw new ValidationException($"Cannot delete farm {farmId}. It has active fields. Deactivate all fields first.");

            var deleted = await _farmRepository.DeleteFarmAsync(farmId);

            if (deleted)
                _logger.LogInformation("Farm {FarmId} deleted successfully", farmId);

            return deleted;
        }
    }
}
