using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // #SOLID - Single Responsibility Principle (SRP)
    // CropSeasonRepository tem uma única responsabilidade: gerenciar a persistência de safras.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Implementa a interface ICropSeasonRepository definida no domínio.
    
    // #SOLID - Liskov Substitution Principle (LSP)
    // CropSeasonRepository pode ser substituído por qualquer outra implementação de ICropSeasonRepository.
    public class CropSeasonRepository : ICropSeasonRepository
    {
        private readonly FarmsDbContext _context;

        public CropSeasonRepository(FarmsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<CropSeason>> GetAllCropSeasonsAsync()
        {
            return await _context.CropSeasons
                .Include(cs => cs.Field)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CropSeason?> GetCropSeasonByIdAsync(int cropSeasonId)
        {
            return await _context.CropSeasons
                .Include(cs => cs.Field)
                    .ThenInclude(f => f.Farm)
                .AsNoTracking()
                .FirstOrDefaultAsync(cs => cs.Id == cropSeasonId);
        }

        public async Task<IEnumerable<CropSeason>> GetCropSeasonsByFieldIdAsync(int fieldId)
        {
            return await _context.CropSeasons
                .AsNoTracking()
                .Where(cs => cs.FieldId == fieldId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CropSeason>> GetCropSeasonsByStatusAsync(CropSeasonStatus status)
        {
            return await _context.CropSeasons
                .Include(cs => cs.Field)
                .AsNoTracking()
                .Where(cs => cs.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<CropSeason>> GetOverdueCropSeasonsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.CropSeasons
                .Include(cs => cs.Field)
                .AsNoTracking()
                .Where(cs => cs.Status != CropSeasonStatus.Finished && 
                            cs.ExpectedHarvestDate < now)
                .ToListAsync();
        }

        public async Task<CropSeason> AddCropSeasonAsync(CropSeason cropSeason)
        {
            cropSeason.CreatedAt = DateTime.UtcNow;
            await _context.CropSeasons.AddAsync(cropSeason);
            await _context.SaveChangesAsync();
            return cropSeason;
        }

        public async Task<CropSeason> UpdateCropSeasonAsync(CropSeason cropSeason)
        {
            // Detach all tracked entities to avoid conflicts
            var trackedCropSeason = _context.ChangeTracker.Entries<CropSeason>()
                .Where(e => e.Entity.Id == cropSeason.Id)
                .ToList();

            foreach (var entry in trackedCropSeason)
                entry.State = EntityState.Detached;

            var trackedFields = _context.ChangeTracker.Entries<Field>()
                .ToList();

            foreach (var entry in trackedFields)
                entry.State = EntityState.Detached;

            var trackedFarms = _context.ChangeTracker.Entries<Farm>()
                .ToList();

            foreach (var entry in trackedFarms)
                entry.State = EntityState.Detached;

            cropSeason.UpdatedAt = DateTime.UtcNow;
            _context.CropSeasons.Update(cropSeason);
            await _context.SaveChangesAsync();
            return cropSeason;
        }

        public async Task<bool> DeleteCropSeasonAsync(int cropSeasonId)
        {
            var cropSeason = await _context.CropSeasons
                .FirstOrDefaultAsync(cs => cs.Id == cropSeasonId);

            if (cropSeason == null)
                return false;

            _context.CropSeasons.Remove(cropSeason);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasDateConflictAsync(
            int fieldId, 
            DateTime plantingDate, 
            DateTime expectedHarvestDate, 
            int? excludeCropSeasonId = null)
        {
            // Verifica se existe alguma safra no mesmo campo com datas sobrepostas
            // Considera apenas safras Planned e Active (não Finished ou Cancelled)
            var query = _context.CropSeasons
                .Where(cs => cs.FieldId == fieldId)
                .Where(cs => cs.Status == CropSeasonStatus.Planned || cs.Status == CropSeasonStatus.Active);

            // Exclui a safra específica se fornecido (útil para updates)
            if (excludeCropSeasonId.HasValue)
                query = query.Where(cs => cs.Id != excludeCropSeasonId.Value);

            return await query.AnyAsync(cs =>
                // Novo período começa antes do fim de uma safra existente
                // E termina depois do início de uma safra existente
                plantingDate < cs.ExpectedHarvestDate &&
                expectedHarvestDate > cs.PlantingDate
            );
        }
    }
}
