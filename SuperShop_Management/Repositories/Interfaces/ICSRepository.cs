using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface ICSRepository : IGenericRepository<ComparativeStatement>
    {
        Task<IEnumerable<ComparativeStatement>> GetActiveCSAsync();
        Task<ComparativeStatement?> GetByRFQIdAsync(int rfqId);
        Task<ComparativeStatement?> GetByCSNumberAsync(string csNumber);
        Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null);
    }
}