using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder>
    {
        Task<IEnumerable<PurchaseOrder>> GetActivePurchaseOrdersAsync();
        Task<PurchaseOrder?> GetByPONumberAsync(string poNumber);
        Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(int supplierId);
        Task<IEnumerable<PurchaseOrder>> GetByCSIdAsync(int csId);
        Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id);
        Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null);
    }
}