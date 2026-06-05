using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class RequisitionRepository : GenericRepository<PurchaseRequisition>, IRequisitionRepository
    {
        public RequisitionRepository(AppDbContext context) : base(context)
        {
        }

        // Override GetByIdAsync to include navigation properties
        public override async Task<PurchaseRequisition?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.RequisitionItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<PurchaseRequisition>> GetActiveRequisitionsAsync()
        {
            return await _dbSet
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.RequisitionItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.RequisitionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseRequisition>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _dbSet
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.RequisitionItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Where(r => r.DepartmentId == departmentId && r.IsActive)
                .OrderByDescending(r => r.RequisitionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseRequisition>> GetByRequestedByIdAsync(int requestedById)
        {
            return await _dbSet
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.RequisitionItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .Where(r => r.RequestedById == requestedById && r.IsActive)
                .OrderByDescending(r => r.RequisitionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseRequisition>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Where(r => r.Status == status && r.IsActive)
                .OrderByDescending(r => r.RequisitionDate)
                .ToListAsync();
        }

        public async Task<PurchaseRequisition?> GetByNumberAsync(string number)
        {
            return await _dbSet
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.RequisitionItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(r => r.RequisitionNumber == number && r.IsActive);
        }

        public async Task<IEnumerable<RequisitionItem>> GetItemsByRequisitionIdAsync(int requisitionId)
        {
            return await _context.RequisitionItems
                .Include(i => i.Product)
                .Where(i => i.RequisitionId == requisitionId && i.IsActive)
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null)
        {
            var query = _dbSet.Where(r => r.RequisitionNumber == number && r.IsActive);
            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}