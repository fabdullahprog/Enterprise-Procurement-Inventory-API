using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<IEnumerable<Department>> GetActiveDepartmentsAsync();
        Task<Department?> GetByCodeAsync(string code);
        Task<bool> IsDuplicateAsync(string code, int? excludeId = null);
    }
}