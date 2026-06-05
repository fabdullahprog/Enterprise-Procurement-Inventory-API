using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface ICurrencyRepository : IGenericRepository<Currency>
    {
        Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
        Task<Currency?> GetByCodeAsync(string code);
        Task<Currency?> GetBaseCurrencyAsync();
        Task<bool> IsDuplicateCodeAsync(string code, int? excludeId = null);
    }
}