using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class EmployeeRequisitionRepository : GenericRepository<Requisition>, IEmployeeRequisitionRepository
    {
        public EmployeeRequisitionRepository(AppDbContext context) : base(context)
        {
        }

        // Override to include Items collection
        public override async Task<Requisition?> GetByIdAsync(int id)
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // Override to include Items collection
        public override async Task<IEnumerable<Requisition>> GetAllAsync()
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Requisition>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Where(r => r.DepartmentId == departmentId && r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Requisition>> GetByRequestedByIdAsync(int userId)
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Where(r => r.RequestedBy == userId && r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Requisition>> GetByStatusAsync(string status)
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Where(r => r.Status == status && r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Requisition>> GetPendingForDepartmentAsync(int departmentId)
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Where(r => r.DepartmentId == departmentId && r.Status == "pending_dept_head" && r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Requisition>> GetForwardedToStoreAsync()
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                    .ThenInclude(i => i.Item)
                        .ThenInclude(p => p!.ItemCategory)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Where(r => r.Status == "forwarded_to_store" && r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<Requisition?> GetForwardedToStoreByIdAsync(int id)
        {
            return await _context.Set<Requisition>()
                .Include(r => r.Items)
                    .ThenInclude(i => i.Item)
                        .ThenInclude(p => p!.ItemCategory)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == "forwarded_to_store" && r.IsActive);
        }
    }
}
