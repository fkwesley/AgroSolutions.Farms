using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // #SOLID - Single Responsibility Principle (SRP)
    // FieldRepository tem uma única responsabilidade: gerenciar a persistência de campos.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Implementa a interface IFieldRepository definida no domínio.
    
    // #SOLID - Liskov Substitution Principle (LSP)
    // FieldRepository pode ser substituído por qualquer outra implementação de IFieldRepository.
    public class FieldRepository : IFieldRepository
    {
        private readonly FarmsDbContext _context;

        public FieldRepository(FarmsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Field>> GetAllFieldsAsync()
        {
            return await _context.Fields
                .Include(f => f.CropSeasons)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Field?> GetFieldByIdAsync(int fieldId)
        {
            return await _context.Fields
                .Include(f => f.CropSeasons)
                .Include(f => f.Farm)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == fieldId);
        }

        public async Task<IEnumerable<Field>> GetFieldsByFarmIdAsync(int farmId)
        {
            return await _context.Fields
                .Include(f => f.CropSeasons)
                .AsNoTracking()
                .Where(f => f.FarmId == farmId)
                .ToListAsync();
        }

        public async Task<Field> AddFieldAsync(Field field)
        {
            field.CreatedAt = DateTime.UtcNow;
            await _context.Fields.AddAsync(field);
            await _context.SaveChangesAsync();
            return field;
        }

        public async Task<Field> UpdateFieldAsync(Field field)
        {
            var trackedEntity = _context.ChangeTracker.Entries<Field>()
                .FirstOrDefault(e => e.Entity.Id == field.Id);

            if (trackedEntity != null)
                trackedEntity.State = EntityState.Detached;

            field.UpdatedAt = DateTime.UtcNow;
            _context.Fields.Update(field);
            await _context.SaveChangesAsync();
            return field;
        }

        public async Task<bool> DeleteFieldAsync(int fieldId)
        {
            var field = await _context.Fields
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null)
                return false;

            _context.Fields.Remove(field);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FieldExistsAsync(int fieldId)
        {
            return await _context.Fields
                .AnyAsync(f => f.Id == fieldId);
        }

        public async Task<decimal> GetTotalFieldsAreaByFarmIdAsync(int farmId, int? excludeFieldId = null)
        {
            var query = _context.Fields
                .Where(f => f.FarmId == farmId);

            if (excludeFieldId.HasValue)
                query = query.Where(f => f.Id != excludeFieldId.Value);

            return await query.SumAsync(f => f.AreaHectares);
        }
    }
}
