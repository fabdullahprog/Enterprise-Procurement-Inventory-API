using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IStockMovementRepository : IGenericRepository<StockMovement>
    {
        Task<IEnumerable<StockMovement>> GetByInventoryIdAsync(int inventoryId);
    }
}