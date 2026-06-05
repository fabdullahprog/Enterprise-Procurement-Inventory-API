using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class BatchRepository : GenericRepository<Batch>, IBatchRepository
    {
        public BatchRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Batch>> GetActiveBatchesAsync()
        {
            return await _dbSet
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .Where(b => b.IsActive && b.Status == "Active")
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Batch>> GetByProductIdAsync(int productId)
        {
            return await _dbSet
                .Where(b => b.ProductId == productId && b.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Batch>> GetExpiringBatchesAsync(int daysThreshold)
        {
            var thresholdDate = DateTime.Now.AddDays(daysThreshold);
            return await _dbSet
                .Where(b => b.ExpiryDate <= thresholdDate && b.Status == "Active")
                .ToListAsync();
        }
    }
}