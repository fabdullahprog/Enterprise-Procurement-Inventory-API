using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        Task<IEnumerable<Brand>> GetActiveBrandsAsync();
        Task<IEnumerable<Brand>> GetBySubCategoryIdAsync(int subCategoryId);
        Task<Brand?> GetByNameAsync(string name);
        Task<bool> IsDuplicateAsync(string name, int? excludeId = null);
    }
}