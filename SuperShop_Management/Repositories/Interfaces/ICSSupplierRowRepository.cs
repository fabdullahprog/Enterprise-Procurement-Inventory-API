using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface ICSSupplierRowRepository : IGenericRepository<CSSupplierRow>
    {
        Task<IEnumerable<CSSupplierRow>> GetByCSIdAsync(int csId);
        Task<IEnumerable<CSSupplierRow>> GetByCSItemIdAsync(int csItemId);
        Task<CSSupplierRow?> GetSelectedRowForItemAsync(int csItemId);
        Task<IEnumerable<CSSupplierRow>> GetSelectedRowsForCSAsync(int csId);
    }
}
