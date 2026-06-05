using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class GRNRepository : GenericRepository<GRN>, IGRNRepository
    {
        public GRNRepository(AppDbContext context) : base(context) { }

        public override async Task<GRN?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(g => g.PurchaseOrder)
                    .ThenInclude(po => po!.Supplier)
                .Include(g => g.ReceivedBy)
                .Include(g => g.StoreApprovedBy)
                .Include(g => g.GRNItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.POItem)
                        .ThenInclude(pi => pi!.Product)
                .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);
        }

        public async Task<IEnumerable<GRN>> GetActiveGRNsAsync()
        {
            return await _dbSet
                .Include(g => g.PurchaseOrder)
                    .ThenInclude(po => po!.Supplier)
                .Include(g => g.ReceivedBy)
                .Include(g => g.StoreApprovedBy)
                .Include(g => g.GRNItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.POItem)
                        .ThenInclude(pi => pi!.Product)
                .Where(g => g.IsActive)
                .OrderByDescending(g => g.ReceivedDate)
                .ToListAsync();
        }

        public async Task<GRN?> GetByGRNNumberAsync(string grnNumber)
        {
            return await _dbSet
                .Include(g => g.PurchaseOrder)
                .Include(g => g.GRNItems)
                .FirstOrDefaultAsync(g => g.GRNNumber == grnNumber && g.IsActive);
        }

        public async Task<IEnumerable<GRN>> GetByPurchaseOrderIdAsync(int poId)
        {
            return await _dbSet
                .Where(g => g.PurchaseOrderId == poId && g.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null)
        {
            var query = _dbSet.Where(g => g.GRNNumber == number && g.IsActive);
            if (excludeId.HasValue)
                query = query.Where(g => g.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}