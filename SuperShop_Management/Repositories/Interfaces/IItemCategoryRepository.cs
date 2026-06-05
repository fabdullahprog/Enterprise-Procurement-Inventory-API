using SuperShop_Management.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IItemCategoryRepository : IGenericRepository<ItemCategory>
    {
        Task<IEnumerable<ItemCategory>> GetActiveCategoriesAsync();
        Task<ItemCategory?> GetByNameAsync(string name);
        Task<bool> IsDuplicateAsync(string name, int? excludeId = null);
    }
}
