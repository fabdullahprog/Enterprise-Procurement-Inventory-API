using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        public BrandRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Brand>> GetActiveBrandsAsync()
        {
            return await _dbSet
                .Include(b => b.SubCategory)
                .Where(b => b.IsActive)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Brand>> GetBySubCategoryIdAsync(int subCategoryId)
        {
            return await _dbSet
                .Include(b => b.SubCategory)
                .Where(b => b.SubCategoryId == subCategoryId && b.IsActive)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
        }

        public async Task<Brand?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(b => b.BrandName == name && b.IsActive);
        }

        public async Task<bool> IsDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(b => b.BrandName == name && b.IsActive);
            if (excludeId.HasValue)
                query = query.Where(b => b.BrandId != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}