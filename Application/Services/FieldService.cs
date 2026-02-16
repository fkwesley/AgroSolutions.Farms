using Application.DTO.Field;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    // #SOLID - Single Responsibility Principle (SRP)
    // FieldService é responsável apenas pela lógica de negócio de campos.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Depende de abstrações (IFieldRepository, IFarmRepository) e não de implementações concretas.
    public class FieldService : IFieldService
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IFarmRepository _farmRepository;
        private readonly ILogger<FieldService> _logger;

        public FieldService(
            IFieldRepository fieldRepository,
            IFarmRepository farmRepository,
            ILogger<FieldService> logger)
        {
            _fieldRepository = fieldRepository ?? throw new ArgumentNullException(nameof(fieldRepository));
            _farmRepository = farmRepository ?? throw new ArgumentNullException(nameof(farmRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<FieldResponse>> GetAllFieldsAsync()
        {
            var fields = await _fieldRepository.GetAllFieldsAsync();
            _logger.LogInformation("Retrieved {FieldCount} fields", fields.Count());
            return fields.Select(f => f.ToResponse()).ToList();
        }

        public async Task<FieldResponse> GetFieldByIdAsync(int fieldId)
        {
            var field = await _fieldRepository.GetFieldByIdAsync(fieldId);

            if (field == null)
                throw new ValidationException($"Field with ID {fieldId} not found.");

            return field.ToResponse();
        }

        public async Task<IEnumerable<FieldResponse>> GetFieldsByFarmIdAsync(int farmId)
        {
            // Valida se a fazenda existe
            if (!(await _farmRepository.GetAllFarmsAsync()).Any(f => f.Id == farmId))
                throw new ValidationException($"Farm with ID {farmId} not found.");

            var fields = await _fieldRepository.GetFieldsByFarmIdAsync(farmId);
            _logger.LogInformation("Retrieved {FieldCount} fields for farm {FarmId}", 
                fields.Count(), farmId);
            return fields.Select(f => f.ToResponse()).ToList();
        }

        public async Task<FieldResponse> AddFieldAsync(AddFieldRequest request)
        {
            // Valida se o Field já existe
            if (await _fieldRepository.FieldExistsAsync(request.Name))
                throw new ValidationException($"Field with name {request.Name} already exists.");

            // Valida se a fazenda existe
            var farm = await _farmRepository.GetFarmByIdAsync(request.FarmId);
            if (farm == null)
                throw new ValidationException($"Farm with ID {request.FarmId} not found.");

            // Valida se a fazenda está ativa
            if (!farm.IsActive)
                throw new ValidationException($"Cannot add field to inactive farm {request.FarmId}.");

            // Valida se há área disponível na fazenda
            var totalFieldsArea = await _fieldRepository.GetTotalFieldsAreaByFarmIdAsync(request.FarmId);
            var availableArea = farm.TotalAreaHectares - totalFieldsArea;

            if (request.AreaHectares > availableArea)
                throw new ValidationException($"Field area {request.AreaHectares} ha exceeds farm total area. Available: {availableArea} ha.");

            var fieldEntity = request.ToEntity();
            var addedField = await _fieldRepository.AddFieldAsync(fieldEntity);

            _logger.LogInformation("Field {FieldId} - {FieldName} added to farm {FarmId}", addedField.Id, addedField.Name, addedField.FarmId);
            return addedField.ToResponse();
        }

        public async Task<FieldResponse> UpdateFieldAsync(UpdateFieldRequest request)
        {
            var existingField = await _fieldRepository.GetFieldByIdAsync(request.Id);

            if (existingField == null)
                throw new ValidationException($"Field with ID {request.Id} not found.");

            // Valida se a fazenda existe e carrega seus dados
            var farm = await _farmRepository.GetFarmByIdAsync(existingField.FarmId);
            if (farm == null)
                throw new ValidationException($"Farm with ID {existingField.FarmId} not found.");

            // Valida se a nova área não excede a área disponível na fazenda
            var totalOtherFieldsArea = await _fieldRepository.GetTotalFieldsAreaByFarmIdAsync(existingField.FarmId, excludeFieldId: request.Id);

            var availableArea = farm.TotalAreaHectares - totalOtherFieldsArea;

            if (request.AreaHectares > availableArea)
                throw new ValidationException($"Field area {request.AreaHectares} ha exceeds farm total area. Available: {availableArea} ha.");

            var fieldEntity = request.ToEntity();
            fieldEntity.CreatedAt = existingField.CreatedAt;
            fieldEntity.CropSeasons = existingField.CropSeasons;

            var updatedField = await _fieldRepository.UpdateFieldAsync(fieldEntity);

            _logger.LogInformation("Field {FieldId} updated successfully", request.Id);
            return updatedField.ToResponse();
        }

        public async Task<bool> DeleteFieldAsync(int fieldId)
        {
            var field = await _fieldRepository.GetFieldByIdAsync(fieldId);

            if (field == null)
                throw new ValidationException($"Field with ID {fieldId} not found.");

            // Validação: não permitir deletar campo com safras ativas
            if (field.CropSeasons.Any(cs => cs.Status == Domain.Enums.CropSeasonStatus.Active))
                throw new ValidationException($"Cannot delete field {fieldId}. It has active crop seasons. Finish or cancel them first.");

            var deleted = await _fieldRepository.DeleteFieldAsync(fieldId);

            if (deleted)
                _logger.LogInformation("Field {FieldId} deleted successfully", fieldId);

            return deleted;
        }
    }
}
