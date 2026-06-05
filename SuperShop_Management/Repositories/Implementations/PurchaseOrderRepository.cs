using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class PurchaseOrderRepository : GenericRepository<PurchaseOrder>, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PurchaseOrder>> GetActivePurchaseOrdersAsync()
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Include(po => po.ComparativeStatement)
                .Include(po => po.CreatedByUser)
                .Include(po => po.ApprovedBy)
                .Include(po => po.POItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Where(po => po.IsActive)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Include(po => po.ComparativeStatement)
                .Include(po => po.CreatedByUser)
                .Include(po => po.ApprovedBy)
                .Include(po => po.POItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == id && po.IsActive);
        }

        public async Task<PurchaseOrder?> GetByPONumberAsync(string poNumber)
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Include(po => po.POItems)
                .FirstOrDefaultAsync(po => po.PONumber == poNumber && po.IsActive);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(int supplierId)
        {
            return await _dbSet
                .Include(po => po.POItems)
                .Where(po => po.SupplierId == supplierId && po.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseOrder>> GetByCSIdAsync(int csId)
        {
            return await _dbSet
                .Where(po => po.ComparativeStatementId == csId && po.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null)
        {
            var query = _dbSet.Where(po => po.PONumber == number && po.IsActive);
            if (excludeId.HasValue)
                query = query.Where(po => po.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}