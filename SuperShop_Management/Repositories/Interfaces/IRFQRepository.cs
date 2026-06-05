using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IRFQRepository : IGenericRepository<RequestForQuotation>
    {
        Task<IEnumerable<RequestForQuotation>> GetActiveRFQsAsync();
        Task<IEnumerable<RequestForQuotation>> GetByRequisitionIdAsync(int requisitionId);
        Task<RequestForQuotation?> GetByRFQNumberAsync(string rfqNumber);
        Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null);
    }
}