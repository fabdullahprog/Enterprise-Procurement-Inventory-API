using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            return await _dbSet
                .Include(s => s.Currency)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.Name == name && s.IsActive);
        }

        public async Task<bool> IsDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(s => s.Name == name && s.IsActive);
            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}