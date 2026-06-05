using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IRequisitionRepository : IGenericRepository<PurchaseRequisition>
    {
        Task<IEnumerable<PurchaseRequisition>> GetActiveRequisitionsAsync();
        Task<IEnumerable<PurchaseRequisition>> GetByDepartmentIdAsync(int departmentId);
        Task<IEnumerable<PurchaseRequisition>> GetByRequestedByIdAsync(int requestedById);
        Task<IEnumerable<PurchaseRequisition>> GetByStatusAsync(string status);
        Task<PurchaseRequisition?> GetByNumberAsync(string number);
        Task<IEnumerable<RequisitionItem>> GetItemsByRequisitionIdAsync(int requisitionId);
        Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null);
    }
}