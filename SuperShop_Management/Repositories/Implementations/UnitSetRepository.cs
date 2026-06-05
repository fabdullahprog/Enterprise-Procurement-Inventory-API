using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class UnitSetRepository : GenericRepository<UnitSet>, IUnitSetRepository
    {
        public UnitSetRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UnitSet>> GetActiveUnitSetsAsync()
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .OrderBy(u => u.NameOfUnitSet)
                .ToListAsync();
        }

        public async Task<UnitSet?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.NameOfUnitSet == name && u.IsActive);
        }

        public async Task<bool> IsDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(u => u.NameOfUnitSet == name && u.IsActive);
            if (excludeId.HasValue)
                query = query.Where(u => u.UnitSetId != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}