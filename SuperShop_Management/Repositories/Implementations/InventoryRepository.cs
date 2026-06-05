using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Inventory>> GetActiveInventoriesAsync()
        {
            return await _dbSet
                .Include(i => i.Product)
                .Include(i => i.Batch)
                .Where(i => i.IsActive)
                .OrderBy(i => i.Product!.Name)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByProductAndBatchAsync(int productId, int batchId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.BatchId == batchId && i.IsActive);
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync()
        {
            return await _dbSet
                .Include(i => i.Product)
                .Where(i => i.AvailableQuantity <= i.MinQuantity && i.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId)
        {
            return await _dbSet
                .Include(i => i.Product)
                .Include(i => i.Batch)
                .Where(i => i.ProductId == productId && i.IsActive)
                .OrderBy(i => i.Batch!.ExpiryDate) // FIFO by expiry date
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetAvailableStockTotalsByProductIdsAsync(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, int>();

            return await _dbSet
                .AsNoTracking()
                .Where(i => ids.Contains(i.ProductId) && i.IsActive)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Total = g.Sum(i => i.AvailableQuantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Total);
        }

        public async Task<IEnumerable<Inventory>> GetByLocationAsync(int? warehouseId, int? floorId, int? zoneId, int? aisleId, int? rackId, int? shelfId, int? binId)
        {
            var query = _dbSet.Include(i => i.Product).Include(i => i.Batch).Where(i => i.IsActive);
            
            if (warehouseId.HasValue) query = query.Where(i => i.WarehouseId == warehouseId);
            if (floorId.HasValue) query = query.Where(i => i.FloorId == floorId);
            if (zoneId.HasValue) query = query.Where(i => i.ZoneId == zoneId);
            if (aisleId.HasValue) query = query.Where(i => i.AisleId == aisleId);
            if (rackId.HasValue) query = query.Where(i => i.RackId == rackId);
            if (shelfId.HasValue) query = query.Where(i => i.ShelfId == shelfId);
            if (binId.HasValue) query = query.Where(i => i.BinId == binId);

            return await query.ToListAsync();
        }
    }
}