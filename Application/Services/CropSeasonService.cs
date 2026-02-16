using Application.DTO.CropSeason;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    // #SOLID - Single Responsibility Principle (SRP)
    // CropSeasonService é responsável apenas pela lógica de negócio de safras.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Depende de abstrações (ICropSeasonRepository, IFieldRepository) e não de implementações concretas.
    public class CropSeasonService : ICropSeasonService
    {
        private readonly ICropSeasonRepository _cropSeasonRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly ILogger<CropSeasonService> _logger;

        public CropSeasonService(
            ICropSeasonRepository cropSeasonRepository,
            IFieldRepository fieldRepository,
            ILogger<CropSeasonService> logger)
        {
            _cropSeasonRepository = cropSeasonRepository ?? throw new ArgumentNullException(nameof(cropSeasonRepository));
            _fieldRepository = fieldRepository ?? throw new ArgumentNullException(nameof(fieldRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CropSeasonResponse>> GetAllCropSeasonsAsync()
        {
            var cropSeasons = await _cropSeasonRepository.GetAllCropSeasonsAsync();
            _logger.LogInformation("Retrieved {CropSeasonCount} crop seasons", cropSeasons.Count());
            return cropSeasons.Select(cs => cs.ToResponse()).ToList();
        }

        public async Task<CropSeasonResponse> GetCropSeasonByIdAsync(string cropSeasonId)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            return cropSeason.ToResponse();
        }

        public async Task<IEnumerable<CropSeasonResponse>> GetCropSeasonsByFieldIdAsync(string fieldId)
        {
            // Valida se o campo existe
            if (!await _fieldRepository.FieldExistsAsync(fieldId))
                throw new ValidationException($"Field with ID {fieldId} not found.");

            var cropSeasons = await _cropSeasonRepository.GetCropSeasonsByFieldIdAsync(fieldId);
            _logger.LogInformation("Retrieved {CropSeasonCount} crop seasons for field {FieldId}", 
                cropSeasons.Count(), fieldId);
            return cropSeasons.Select(cs => cs.ToResponse()).ToList();
        }

        public async Task<IEnumerable<CropSeasonResponse>> GetCropSeasonsByStatusAsync(CropSeasonStatus status)
        {
            var cropSeasons = await _cropSeasonRepository.GetCropSeasonsByStatusAsync(status);
            _logger.LogInformation("Retrieved {CropSeasonCount} crop seasons with status {Status}", 
                cropSeasons.Count(), status);
            return cropSeasons.Select(cs => cs.ToResponse()).ToList();
        }

        public async Task<IEnumerable<CropSeasonResponse>> GetOverdueCropSeasonsAsync()
        {
            var cropSeasons = await _cropSeasonRepository.GetOverdueCropSeasonsAsync();
            _logger.LogWarning("Found {OverdueCount} overdue crop seasons", cropSeasons.Count());
            return cropSeasons.Select(cs => cs.ToResponse()).ToList();
        }

        public async Task<CropSeasonResponse> AddCropSeasonAsync(AddCropSeasonRequest request)
        {
            // Valida se já existe
            if (await _cropSeasonRepository.CropSeasonExistsAsync(request.Id))
                throw new ValidationException($"Crop season with ID {request.Id} already exists.");

            // Valida se o campo existe
            var field = await _fieldRepository.GetFieldByIdAsync(request.FieldId);
            if (field == null)
                throw new ValidationException($"Field with ID {request.FieldId} not found.");

            // Valida se o campo está ativo
            if (!field.IsActive)
                throw new ValidationException($"Cannot add crop season to inactive field {request.FieldId}.");

            var cropSeasonEntity = request.ToEntity();
            var addedCropSeason = await _cropSeasonRepository.AddCropSeasonAsync(cropSeasonEntity);

            _logger.LogInformation("Crop season {CropSeasonId} added to field {FieldId}", 
                request.Id, request.FieldId);
            return addedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> UpdateCropSeasonAsync(UpdateCropSeasonRequest request)
        {
            var existingCropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(request.Id);

            if (existingCropSeason == null)
                throw new ValidationException($"Crop season with ID {request.Id} not found.");

            // Apenas safras planejadas podem ter datas alteradas
            if (existingCropSeason.Status != CropSeasonStatus.Planned)
            {
                throw new ValidationException(
                    $"Cannot update crop season {request.Id}. " +
                    "Only planned crop seasons can be updated.");
            }

            existingCropSeason.UpdateFromRequest(request);
            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(existingCropSeason);

            _logger.LogInformation("Crop season {CropSeasonId} updated successfully", request.Id);
            return updatedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> StartPlantingAsync(string cropSeasonId)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            cropSeason.StartPlanting();
            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(cropSeason);

            _logger.LogInformation("Planting started for crop season {CropSeasonId}", cropSeasonId);
            return updatedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> FinishHarvestAsync(string cropSeasonId, DateTime harvestDate)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            cropSeason.FinishHarvest(harvestDate);
            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(cropSeason);

            _logger.LogInformation("Harvest finished for crop season {CropSeasonId} on {HarvestDate}", 
                cropSeasonId, harvestDate);
            return updatedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> CancelCropSeasonAsync(string cropSeasonId)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            cropSeason.Cancel();
            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(cropSeason);

            _logger.LogInformation("Crop season {CropSeasonId} cancelled", cropSeasonId);
            return updatedCropSeason.ToResponse();
        }

        public async Task<bool> DeleteCropSeasonAsync(string cropSeasonId)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            // Validação: não permitir deletar safra ativa
            if (cropSeason.Status == CropSeasonStatus.Active)
            {
                throw new ValidationException(
                    $"Cannot delete active crop season {cropSeasonId}. " +
                    "Finish or cancel it first.");
            }

            var deleted = await _cropSeasonRepository.DeleteCropSeasonAsync(cropSeasonId);

            if (deleted)
                _logger.LogInformation("Crop season {CropSeasonId} deleted successfully", cropSeasonId);

            return deleted;
        }
    }
}
