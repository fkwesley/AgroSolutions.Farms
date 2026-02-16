using Application.DTO.Field;

namespace Application.Interfaces
{
    // #SOLID - Interface Segregation Principle (ISP)
    // Esta interface define apenas os métodos relacionados a operações de campos.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Esta interface permite que camadas superiores (API) dependam de abstração,
    // não da implementação concreta (FieldService).
    public interface IFieldService
    {
        Task<IEnumerable<FieldResponse>> GetAllFieldsAsync();
        Task<FieldResponse> GetFieldByIdAsync(int fieldId);
        Task<IEnumerable<FieldResponse>> GetFieldsByFarmIdAsync(int farmId);
        Task<FieldResponse> AddFieldAsync(AddFieldRequest request);
        Task<FieldResponse> UpdateFieldAsync(UpdateFieldRequest request);
        Task<bool> DeleteFieldAsync(int fieldId);
    }
}
