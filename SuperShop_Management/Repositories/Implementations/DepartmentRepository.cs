using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
        {
            return await _dbSet
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public async Task<Department?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.DepartmentCode == code && d.IsActive);
        }

        public async Task<bool> IsDuplicateAsync(string code, int? excludeId = null)
        {
            var query = _dbSet.Where(d => d.DepartmentCode == code && d.IsActive);
            if (excludeId.HasValue)
                query = query.Where(d => d.DepartmentId != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}