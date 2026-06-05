using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IUnitRepository : IGenericRepository<Unit>
    {
        Task<IEnumerable<Unit>> GetActiveUnitsAsync();
        Task<IEnumerable<Unit>> GetByUnitSetIdAsync(int unitSetId);
        Task<Unit?> GetBaseUnitOfSetAsync(int unitSetId);
        Task<bool> IsDuplicateAsync(string name, int unitSetId, int? excludeId = null);
    }
}