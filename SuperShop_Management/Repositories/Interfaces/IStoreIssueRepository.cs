using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IStoreIssueRepository : IGenericRepository<StoreIssue>
    {
        Task<IEnumerable<StoreIssue>> GetAllWithDetailsAsync();
        Task<IEnumerable<StoreIssue>> GetByRequisitionIdAsync(int requisitionId);
        Task<IEnumerable<StoreIssue>> GetByStatusAsync(string status);
        Task<StoreIssue?> GetByIdWithDetailsAsync(int id);
    }
}
