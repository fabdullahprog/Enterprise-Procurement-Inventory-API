using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class CSRepository : GenericRepository<ComparativeStatement>, ICSRepository
    {
        public CSRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<ComparativeStatement?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.RFQ)
                .Include(c => c.CreatedByUser)
                .Include(c => c.ApprovedBy)
                .Include(c => c.CSItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Include(c => c.CSItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.SelectedQuotationItem)
                        .ThenInclude(qi => qi!.SupplierQuotation)
                            .ThenInclude(sq => sq!.Supplier)
                // NEW: Also load SupplierRows for the new CS structure
                .Include(c => c.CSItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.SupplierRows.Where(sr => sr.IsActive))
                        .ThenInclude(sr => sr.Supplier)
                .Include(c => c.CSItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.SupplierRows.Where(sr => sr.IsActive))
                        .ThenInclude(sr => sr.QuotationItem!)
                            .ThenInclude(qi => qi.Product)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ComparativeStatement>> GetActiveCSAsync()
        {
            return await _dbSet
                .Include(c => c.RFQ)
                .Include(c => c.CreatedByUser)
                .Include(c => c.ApprovedBy)
                .Include(c => c.CSItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Include(c => c.CSItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.SelectedQuotationItem)
                        .ThenInclude(qi => qi!.SupplierQuotation)
                            .ThenInclude(sq => sq!.Supplier)
                .OrderByDescending(c => c.CSDate)
                .ToListAsync();
        }

        public async Task<ComparativeStatement?> GetByRFQIdAsync(int rfqId)
        {
            return await _dbSet
                .Include(c => c.CSItems.Where(i => i.IsActive))
                .FirstOrDefaultAsync(c => c.RFQId == rfqId);
        }

        public async Task<ComparativeStatement?> GetByCSNumberAsync(string csNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.CSNumber == csNumber && c.IsActive);
        }

        public async Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.CSNumber == number && c.IsActive);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}