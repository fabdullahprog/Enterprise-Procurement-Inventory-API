using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class StockMovementRepository : GenericRepository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StockMovement>> GetByInventoryIdAsync(int inventoryId)
        {
            return await _dbSet
                .Include(s => s.CreatedByUser)          // ✅ Include navigation property
                .Where(s => s.InventoryId == inventoryId && s.IsActive)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }
    }
}