using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class RFQRepository : GenericRepository<RequestForQuotation>, IRFQRepository
    {
        public RFQRepository(AppDbContext context) : base(context)
        {
        }

        // Override to eager-load Requisition with its items and products
        public override async Task<RequestForQuotation?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Requisition)
                    .ThenInclude(req => req.RequisitionItems.Where(i => i.IsActive))
                        .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<RequestForQuotation>> GetActiveRFQsAsync()
        {
            return await _dbSet
                .Include(r => r.Requisition)
                .Include(r => r.CreatedByUser)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.RFQDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestForQuotation>> GetByRequisitionIdAsync(int requisitionId)
        {
            return await _dbSet
                .Include(r => r.Requisition)
                .Where(r => r.RequisitionId == requisitionId && r.IsActive)
                .ToListAsync();
        }

        public async Task<RequestForQuotation?> GetByRFQNumberAsync(string rfqNumber)
        {
            return await _dbSet
                .Include(r => r.Requisition)
                .FirstOrDefaultAsync(r => r.RFQNumber == rfqNumber && r.IsActive);
        }

        public async Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null)
        {
            var query = _dbSet.Where(r => r.RFQNumber == number && r.IsActive);
            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}