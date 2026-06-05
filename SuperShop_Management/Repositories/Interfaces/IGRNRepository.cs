using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IGRNRepository : IGenericRepository<GRN>
    {
        Task<IEnumerable<GRN>> GetActiveGRNsAsync();
        Task<GRN?> GetByGRNNumberAsync(string grnNumber);
        Task<IEnumerable<GRN>> GetByPurchaseOrderIdAsync(int poId);
        Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null);
    }
}