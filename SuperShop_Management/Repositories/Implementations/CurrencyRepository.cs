using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class CurrencyRepository : GenericRepository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public async Task<Currency?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
        }

        public async Task<Currency?> GetBaseCurrencyAsync()
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.IsBaseCurrency && c.IsActive);
        }

        public async Task<bool> IsDuplicateCodeAsync(string code, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Code == code && c.IsActive);
            if (excludeId.HasValue)
                query = query.Where(c => c.CurrencyId != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}