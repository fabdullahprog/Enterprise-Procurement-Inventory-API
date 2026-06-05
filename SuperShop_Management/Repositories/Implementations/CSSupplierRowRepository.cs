using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class CSSupplierRowRepository : GenericRepository<CSSupplierRow>, ICSSupplierRowRepository
    {
        public CSSupplierRowRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CSSupplierRow>> GetByCSIdAsync(int csId)
        {
            return await _context.Set<CSSupplierRow>()
                .Include(csr => csr.CS)
                .Include(csr => csr.CSItem)
                    .ThenInclude(ci => ci.Product)
                .Include(csr => csr.Supplier)
                .Include(csr => csr.QuotationItem)
                    .ThenInclude(qi => qi.Product)
                .Where(csr => csr.CSId == csId && csr.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<CSSupplierRow>> GetByCSItemIdAsync(int csItemId)
        {
            return await _context.Set<CSSupplierRow>()
                .Include(csr => csr.Supplier)
                .Include(csr => csr.QuotationItem)
                .Where(csr => csr.CSItemId == csItemId && csr.IsActive)
                .ToListAsync();
        }

        public async Task<CSSupplierRow?> GetSelectedRowForItemAsync(int csItemId)
        {
            return await _context.Set<CSSupplierRow>()
                .Include(csr => csr.Supplier)
                .Include(csr => csr.QuotationItem)
                .FirstOrDefaultAsync(csr => csr.CSItemId == csItemId && csr.IsSelected && csr.IsActive);
        }

        public async Task<IEnumerable<CSSupplierRow>> GetSelectedRowsForCSAsync(int csId)
        {
            return await _context.Set<CSSupplierRow>()
                .Include(csr => csr.CSItem)
                    .ThenInclude(ci => ci.Product)
                .Include(csr => csr.Supplier)
                .Include(csr => csr.QuotationItem)
                .Where(csr => csr.CSId == csId && csr.IsSelected && csr.IsActive)
                .ToListAsync();
        }
    }
}
