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

        public async Task<CropSeasonResponse> GetCropSeasonByIdAsync(int cropSeasonId)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            return cropSeason.ToResponse();
        }

        public async Task<IEnumerable<CropSeasonResponse>> GetCropSeasonsByFieldIdAsync(int fieldId)
        {
            // Valida se o campo existe
            var field = await _fieldRepository.GetFieldByIdAsync(fieldId);
            if (field == null)
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
            // Valida se o campo existe
            var field = await _fieldRepository.GetFieldByIdAsync(request.FieldId);
            if (field == null)
                throw new ValidationException($"Field with ID {request.FieldId} not found.");

            // Valida se o campo está ativo
            if (!field.IsActive)
                throw new ValidationException($"Cannot add crop season to inactive field {request.FieldId}.");

            // Valida se há conflito de datas (campo ocupado no período)
            if (await _cropSeasonRepository.HasDateConflictAsync(request.FieldId, request.PlantingDate, request.ExpectedHarvestDate))
                throw new ValidationException($"Field {request.FieldId} is not available for the period from {request.PlantingDate:yyyy-MM-dd} " +
                                                    $"to {request.ExpectedHarvestDate:yyyy-MM-dd}. There is an overlapping crop season.");

            var cropSeasonEntity = request.ToEntity();
            cropSeasonEntity.SetCreatedAudit(request.CreatedBy);

            var addedCropSeason = await _cropSeasonRepository.AddCropSeasonAsync(cropSeasonEntity);

            _logger.LogInformation(
                "Crop season {CropSeasonId} added to field {FieldId} for period {PlantingDate} to {ExpectedHarvestDate} by {CreatedBy}", 
                addedCropSeason.Id, request.FieldId, request.PlantingDate, request.ExpectedHarvestDate, request.CreatedBy);
            return addedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> UpdateCropSeasonAsync(UpdateCropSeasonRequest request)
        {
            var existingCropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(request.Id);

            if (existingCropSeason == null)
                throw new ValidationException($"Crop season with ID {request.Id} not found.");

            // Apenas safras planejadas podem ter datas alteradas
            if (existingCropSeason.Status != CropSeasonStatus.Planned)
                throw new ValidationException($"Cannot update crop season {request.Id}. Only planned crop seasons can be updated.");

            // Valida conflito de datas (excluindo a própria safra)
            if (await _cropSeasonRepository.HasDateConflictAsync(
                                                existingCropSeason.FieldId,
                                                request.PlantingDate,
                                                request.ExpectedHarvestDate,
                                                excludeCropSeasonId: request.Id))
                throw new ValidationException($"Field is not available for the updated period " +
                                    $"from {request.PlantingDate:yyyy-MM-dd} to {request.ExpectedHarvestDate:yyyy-MM-dd}. " +
                                    "There is an overlapping crop season.");

            existingCropSeason.UpdateFromRequest(request);
            existingCropSeason.SetUpdatedAudit(request.UpdatedBy);

            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(existingCropSeason);

            _logger.LogInformation("Crop season {CropSeasonId} updated successfully by {UpdatedBy}", request.Id, request.UpdatedBy);
            return updatedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> StartPlantingAsync(int cropSeasonId, string updatedBy)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            cropSeason.StartPlanting();
            cropSeason.SetUpdatedAudit(updatedBy);
            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(cropSeason);

            _logger.LogInformation("Planting started for crop season {CropSeasonId} by {UpdatedBy}", 
                cropSeasonId, updatedBy);
            return updatedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> FinishHarvestAsync(int cropSeasonId, DateTime harvestDate, string updatedBy)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            cropSeason.FinishHarvest(harvestDate);
            cropSeason.SetUpdatedAudit(updatedBy);
            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(cropSeason);

            _logger.LogInformation("Harvest finished for crop season {CropSeasonId} on {HarvestDate} by {UpdatedBy}", 
                cropSeasonId, harvestDate, updatedBy);
            return updatedCropSeason.ToResponse();
        }

        public async Task<CropSeasonResponse> CancelCropSeasonAsync(int cropSeasonId, string updatedBy)
        {
            var cropSeason = await _cropSeasonRepository.GetCropSeasonByIdAsync(cropSeasonId);

            if (cropSeason == null)
                throw new ValidationException($"Crop season with ID {cropSeasonId} not found.");

            cropSeason.Cancel();
            cropSeason.SetUpdatedAudit(updatedBy);
            var updatedCropSeason = await _cropSeasonRepository.UpdateCropSeasonAsync(cropSeason);

            _logger.LogInformation("Crop season {CropSeasonId} cancelled by {UpdatedBy}", 
                cropSeasonId, updatedBy);
            return updatedCropSeason.ToResponse();
        }

        public async Task<bool> DeleteCropSeasonAsync(int cropSeasonId)
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
