using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class StoreIssueRepository : GenericRepository<StoreIssue>, IStoreIssueRepository
    {
        public StoreIssueRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StoreIssue>> GetAllWithDetailsAsync()
        {
            return await _context.Set<StoreIssue>()
                .Include(si => si.Requisition)
                    .ThenInclude(r => r.Items)
                .Include(si => si.IssuedBy)
                .Where(si => si.IsActive)
                .OrderByDescending(si => si.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StoreIssue>> GetByRequisitionIdAsync(int requisitionId)
        {
            return await _context.Set<StoreIssue>()
                .Include(si => si.Requisition)
                    .ThenInclude(r => r.Items)
                .Include(si => si.IssuedBy)
                .Where(si => si.RequisitionId == requisitionId && si.IsActive)
                .OrderByDescending(si => si.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StoreIssue>> GetByStatusAsync(string status)
        {
            return await _context.Set<StoreIssue>()
                .Include(si => si.Requisition)
                    .ThenInclude(r => r.Items)
                .Include(si => si.IssuedBy)
                .Where(si => si.Status == status && si.IsActive)
                .OrderByDescending(si => si.CreatedDate)
                .ToListAsync();
        }

        public async Task<StoreIssue?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<StoreIssue>()
                .Include(si => si.Requisition)
                    .ThenInclude(r => r.Items)
                .Include(si => si.Requisition)
                    .ThenInclude(r => r.RequestedByUser)
                .Include(si => si.Requisition)
                    .ThenInclude(r => r.Department)
                .Include(si => si.IssuedBy)
                .FirstOrDefaultAsync(si => si.Id == id && si.IsActive);
        }
    }
}
