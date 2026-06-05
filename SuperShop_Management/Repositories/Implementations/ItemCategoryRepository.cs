using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class ItemCategoryRepository : GenericRepository<ItemCategory>, IItemCategoryRepository
    {
        public ItemCategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ItemCategory>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<ItemCategory?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.CategoryName == name && c.IsActive);
        }

        public async Task<bool> IsDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.CategoryName == name && c.IsActive);
            if (excludeId.HasValue)
                query = query.Where(c => c.ItemCategoryId != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}
