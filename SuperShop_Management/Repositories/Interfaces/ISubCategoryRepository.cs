using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface ISubCategoryRepository : IGenericRepository<SubCategory>
    {
        Task<IEnumerable<SubCategory>> GetActiveSubCategoriesAsync();
        Task<IEnumerable<SubCategory>> GetByCategoryIdAsync(int categoryId);
        Task<SubCategory?> GetByNameAsync(string name);
        Task<bool> IsDuplicateAsync(string name, int? excludeId = null);
    }
}