using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // #SOLID - Single Responsibility Principle (SRP)
    // FarmRepository tem uma única responsabilidade: gerenciar a persistência de fazendas.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Implementa a interface IFarmRepository definida no domínio.
    
    // #SOLID - Liskov Substitution Principle (LSP)
    // FarmRepository pode ser substituído por qualquer outra implementação de IFarmRepository.
    public class FarmRepository : IFarmRepository
    {
        private readonly FarmsDbContext _context;

        public FarmRepository(FarmsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Farm>> GetAllFarmsAsync()
        {
            return await _context.Farms
                .Include(f => f.Fields)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Farm?> GetFarmByIdAsync(int farmId)
        {
            return await _context.Farms
                .Include(f => f.Fields)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == farmId);
        }

        public async Task<IEnumerable<Farm>> GetFarmsByProducerIdAsync(string producerId)
        {
            return await _context.Farms
                .Include(f => f.Fields)
                .AsNoTracking()
                .Where(f => f.ProducerId.ToUpper() == producerId.ToUpper())
                .ToListAsync();
        }

        public async Task<Farm> AddFarmAsync(Farm farm)
        {
            await _context.Farms.AddAsync(farm);
            await _context.SaveChangesAsync();
            return farm;
        }

        public async Task<Farm> UpdateFarmAsync(Farm farm)
        {
            var trackedEntity = _context.ChangeTracker.Entries<Farm>()
                .FirstOrDefault(e => e.Entity.Id == farm.Id);

            if (trackedEntity != null)
                trackedEntity.State = EntityState.Detached;

            farm.UpdatedAt = DateTime.UtcNow;
            _context.Farms.Update(farm);
            await _context.SaveChangesAsync();
            return farm;
        }

        public async Task<bool> DeleteFarmAsync(int farmId)
        {
            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.Id == farmId);

            if (farm == null)
                return false;

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FarmExistsAsync(int farmId)
        {
            return await _context.Farms
                .AnyAsync(f => f.Id == farmId);
        }
    }
}
