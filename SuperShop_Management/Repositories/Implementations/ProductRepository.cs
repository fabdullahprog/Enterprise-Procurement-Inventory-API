using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Include(p => p.ItemCategory)
                .Include(p => p.SubCategory)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetByBarcodeAsync(string barcode)
        {
            return await _dbSet
                .Include(p => p.ItemCategory)
                .Include(p => p.SubCategory)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.ItemCategory)
                .Include(p => p.SubCategory)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Where(p => p.ItemCategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByBrandIdAsync(int brandId)
        {
            return await _dbSet
                .Include(p => p.ItemCategory)
                .Include(p => p.SubCategory)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Where(p => p.BrandId == brandId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateBarcodeAsync(string barcode, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Barcode == barcode && p.IsActive);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}