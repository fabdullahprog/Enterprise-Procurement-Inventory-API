using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface ISupplierQuotationRepository : IGenericRepository<SupplierQuotation>
    {
        Task<IEnumerable<SupplierQuotation>> GetActiveQuotationsAsync();
        Task<IEnumerable<SupplierQuotation>> GetByRFQIdAsync(int rfqId);
        Task<SupplierQuotation?> GetByQuotationNumberAsync(string number);
        Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null);
    }
}