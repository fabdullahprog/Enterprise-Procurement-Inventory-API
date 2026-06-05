using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IUnitSetRepository : IGenericRepository<UnitSet>
    {
        Task<IEnumerable<UnitSet>> GetActiveUnitSetsAsync();
        Task<UnitSet?> GetByNameAsync(string name);
        Task<bool> IsDuplicateAsync(string name, int? excludeId = null);
    }
}