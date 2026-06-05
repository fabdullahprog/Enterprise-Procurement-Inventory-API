using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class RFQSupplierRepository : GenericRepository<RFQSupplier>, IRFQSupplierRepository
    {
        public RFQSupplierRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RFQSupplier>> GetByRFQIdAsync(int rfqId)
        {
            return await _context.Set<RFQSupplier>()
                .Include(rs => rs.RFQ)
                .Include(rs => rs.Supplier)
                .Where(rs => rs.RFQId == rfqId && rs.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<RFQSupplier>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.Set<RFQSupplier>()
                .Include(rs => rs.RFQ)
                .Include(rs => rs.Supplier)
                .Where(rs => rs.SupplierId == supplierId && rs.IsActive)
                .OrderByDescending(rs => rs.SentAt)
                .ToListAsync();
        }

        public async Task<bool> IsSupplierInRFQAsync(int rfqId, int supplierId)
        {
            return await _context.Set<RFQSupplier>()
                .AnyAsync(rs => rs.RFQId == rfqId && rs.SupplierId == supplierId && rs.IsActive);
        }
    }
}
