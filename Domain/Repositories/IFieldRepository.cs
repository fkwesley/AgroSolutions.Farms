using Domain.Entities;

namespace Domain.Repositories
{
    public interface IFieldRepository
    {
        Task<IEnumerable<Field>> GetAllFieldsAsync();
        Task<Field?> GetFieldByIdAsync(int fieldId);
        Task<IEnumerable<Field>> GetFieldsByFarmIdAsync(int farmId);
        Task<Field> AddFieldAsync(Field field);
        Task<Field> UpdateFieldAsync(Field field);
        Task<bool> DeleteFieldAsync(int fieldId);
        Task<bool> FieldExistsAsync(string fieldName);
        Task<decimal> GetTotalFieldsAreaByFarmIdAsync(int farmId, int? excludeFieldId = null);
    }
}
