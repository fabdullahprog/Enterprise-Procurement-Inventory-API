using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class QuotationItemRepository : GenericRepository<QuotationItem>, IQuotationItemRepository
    {
        public QuotationItemRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<QuotationItem>> GetByQuotationIdAsync(int supplierQuotationId)
        {
            return await _dbSet
                .Include(q => q.Product)
                .Where(q => q.SupplierQuotationId == supplierQuotationId && q.IsActive)
                .ToListAsync();
        }
    }
}