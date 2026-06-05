using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class QualityCheckRepository : GenericRepository<QualityCheck>, IQualityCheckRepository
    {
        public QualityCheckRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<QualityCheck>> GetActiveQCsAsync()
        {
            return await _dbSet
                .Include(q => q.GRN)
                .Include(q => q.InspectedBy)
                .Include(q => q.QCItems.Where(i => i.IsActive))
                    .ThenInclude(i => i.GRNItem)
                        .ThenInclude(gi => gi!.POItem)
                            .ThenInclude(pi => pi!.Product)
                .Where(q => q.IsActive)
                .OrderByDescending(q => q.QCDate)
                .ToListAsync();
        }

        public async Task<QualityCheck?> GetByQCNumberAsync(string qcNumber)
        {
            return await _dbSet
                .Include(q => q.GRN)
                .Include(q => q.QCItems)
                .FirstOrDefaultAsync(q => q.QCNumber == qcNumber && q.IsActive);
        }

        public async Task<QualityCheck?> GetByGRNIdAsync(int grnId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(q => q.GRNId == grnId && q.IsActive);
        }

        public async Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null)
        {
            var query = _dbSet.Where(q => q.QCNumber == number && q.IsActive);
            if (excludeId.HasValue)
                query = query.Where(q => q.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}