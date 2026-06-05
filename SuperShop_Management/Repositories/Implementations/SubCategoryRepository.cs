using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class SubCategoryRepository : GenericRepository<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SubCategory>> GetActiveSubCategoriesAsync()
        {
            return await _dbSet
                .Include(s => s.ItemCategory)
                .Where(s => s.IsActive)
                .OrderBy(s => s.SubCategoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubCategory>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(s => s.ItemCategory)
                .Where(s => s.ItemCategoryId == categoryId && s.IsActive)
                .OrderBy(s => s.SubCategoryName)
                .ToListAsync();
        }

        public async Task<SubCategory?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.SubCategoryName == name && s.IsActive);
        }

        public async Task<bool> IsDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(s => s.SubCategoryName == name && s.IsActive);
            if (excludeId.HasValue)
                query = query.Where(s => s.SubCategoryId != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}