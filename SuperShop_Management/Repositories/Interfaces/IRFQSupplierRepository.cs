using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IRFQSupplierRepository : IGenericRepository<RFQSupplier>
    {
        Task<IEnumerable<RFQSupplier>> GetByRFQIdAsync(int rfqId);
        Task<IEnumerable<RFQSupplier>> GetBySupplierIdAsync(int supplierId);
        Task<bool> IsSupplierInRFQAsync(int rfqId, int supplierId);
    }
}
