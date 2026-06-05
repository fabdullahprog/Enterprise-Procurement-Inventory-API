using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class SupplierQuotationRepository : GenericRepository<SupplierQuotation>, ISupplierQuotationRepository
    {
        public SupplierQuotationRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<SupplierQuotation?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(q => q.RFQ)
                .Include(q => q.Supplier)
                .Include(q => q.Currency)
                .Include(q => q.QuotationItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(q => q.Id == id && q.IsActive);
        }

        public async Task<IEnumerable<SupplierQuotation>> GetActiveQuotationsAsync()
        {
            return await _dbSet
                .Include(q => q.RFQ)
                .Include(q => q.Supplier)
                .Include(q => q.Currency)
                .Include(q => q.QuotationItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Where(q => q.IsActive)
                .OrderByDescending(q => q.QuotationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupplierQuotation>> GetByRFQIdAsync(int rfqId)
        {
            return await _dbSet
                .Include(q => q.Supplier)
                .Include(q => q.QuotationItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Where(q => q.RFQId == rfqId && q.IsActive)
                .ToListAsync();
        }

        public async Task<SupplierQuotation?> GetByQuotationNumberAsync(string number)
        {
            return await _dbSet
                .Include(q => q.RFQ)
                .Include(q => q.Supplier)
                .FirstOrDefaultAsync(q => q.QuotationNumber == number && q.IsActive);
        }

        public async Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null)
        {
            var query = _dbSet.Where(q => q.QuotationNumber == number && q.IsActive);
            if (excludeId.HasValue)
                query = query.Where(q => q.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}