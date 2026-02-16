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

        public async Task<CropSeason?> GetCropSeasonByIdAsync(string cropSeasonId)
        {
            return await _context.CropSeasons
                .Include(cs => cs.Field)
                    .ThenInclude(f => f.Farm)
                .AsNoTracking()
                .FirstOrDefaultAsync(cs => cs.Id.ToUpper() == cropSeasonId.ToUpper());
        }

        public async Task<IEnumerable<CropSeason>> GetCropSeasonsByFieldIdAsync(string fieldId)
        {
            return await _context.CropSeasons
                .AsNoTracking()
                .Where(cs => cs.FieldId.ToUpper() == fieldId.ToUpper())
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
            var trackedEntity = _context.ChangeTracker.Entries<CropSeason>()
                .FirstOrDefault(e => e.Entity.Id == cropSeason.Id);

            if (trackedEntity != null)
                trackedEntity.State = EntityState.Detached;

            cropSeason.UpdatedAt = DateTime.UtcNow;
            _context.CropSeasons.Update(cropSeason);
            await _context.SaveChangesAsync();
            return cropSeason;
        }

        public async Task<bool> DeleteCropSeasonAsync(string cropSeasonId)
        {
            var cropSeason = await _context.CropSeasons
                .FirstOrDefaultAsync(cs => cs.Id.ToUpper() == cropSeasonId.ToUpper());

            if (cropSeason == null)
                return false;

            _context.CropSeasons.Remove(cropSeason);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CropSeasonExistsAsync(string cropSeasonId)
        {
            return await _context.CropSeasons
                .AnyAsync(cs => cs.Id.ToUpper() == cropSeasonId.ToUpper());
        }
    }
}
