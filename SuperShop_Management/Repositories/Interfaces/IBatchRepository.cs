using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IBatchRepository : IGenericRepository<Batch>
    {
        Task<IEnumerable<Batch>> GetActiveBatchesAsync();
        Task<IEnumerable<Batch>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Batch>> GetExpiringBatchesAsync(int daysThreshold);
    }
}