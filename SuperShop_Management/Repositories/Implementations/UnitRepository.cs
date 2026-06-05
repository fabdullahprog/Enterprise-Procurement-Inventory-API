using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class UnitRepository : GenericRepository<Unit>, IUnitRepository
    {
        public UnitRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Unit>> GetActiveUnitsAsync()
        {
            return await _dbSet
                .Include(u => u.UnitSet)
                .Where(u => u.IsActive)
                .OrderBy(u => u.NameOfUnit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Unit>> GetByUnitSetIdAsync(int unitSetId)
        {
            return await _dbSet
                .Include(u => u.UnitSet)
                .Where(u => u.UnitSetId == unitSetId && u.IsActive)
                .OrderBy(u => u.NameOfUnit)
                .ToListAsync();
        }

        public async Task<Unit?> GetBaseUnitOfSetAsync(int unitSetId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.UnitSetId == unitSetId && u.IsBaseUnit && u.IsActive);
        }

        public async Task<bool> IsDuplicateAsync(string name, int unitSetId, int? excludeId = null)
        {
            var query = _dbSet.Where(u => u.NameOfUnit == name && u.UnitSetId == unitSetId && u.IsActive);
            if (excludeId.HasValue)
                query = query.Where(u => u.UnitId != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}