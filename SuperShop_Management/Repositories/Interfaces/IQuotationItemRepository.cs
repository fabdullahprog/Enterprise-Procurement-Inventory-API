using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IQuotationItemRepository : IGenericRepository<QuotationItem>
    {
        Task<IEnumerable<QuotationItem>> GetByQuotationIdAsync(int supplierQuotationId);
    }
}