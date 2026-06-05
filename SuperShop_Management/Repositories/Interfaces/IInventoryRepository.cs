using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
        Task<IEnumerable<Inventory>> GetActiveInventoriesAsync();
        Task<Inventory?> GetByProductAndBatchAsync(int productId, int batchId);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync();
        Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId);
        Task<Dictionary<int, int>> GetAvailableStockTotalsByProductIdsAsync(IEnumerable<int> productIds);
        Task<IEnumerable<Inventory>> GetByLocationAsync(int? warehouseId, int? floorId, int? zoneId, int? aisleId, int? rackId, int? shelfId, int? binId);
    }
}