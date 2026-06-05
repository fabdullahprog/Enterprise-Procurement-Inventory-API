using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IQualityCheckRepository : IGenericRepository<QualityCheck>
    {
        Task<IEnumerable<QualityCheck>> GetActiveQCsAsync();
        Task<QualityCheck?> GetByQCNumberAsync(string qcNumber);
        Task<QualityCheck?> GetByGRNIdAsync(int grnId);
        Task<bool> IsDuplicateNumberAsync(string number, int? excludeId = null);
    }
}