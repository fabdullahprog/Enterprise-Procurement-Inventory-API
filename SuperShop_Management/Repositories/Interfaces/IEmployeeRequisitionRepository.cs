using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IEmployeeRequisitionRepository : IGenericRepository<Requisition>
    {
        Task<IEnumerable<Requisition>> GetByDepartmentIdAsync(int departmentId);
        Task<IEnumerable<Requisition>> GetByRequestedByIdAsync(int userId);
        Task<IEnumerable<Requisition>> GetByStatusAsync(string status);
        Task<IEnumerable<Requisition>> GetPendingForDepartmentAsync(int departmentId);
        Task<IEnumerable<Requisition>> GetForwardedToStoreAsync();
        Task<Requisition?> GetForwardedToStoreByIdAsync(int id);
    }
}
