using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class GRNItemRepository : GenericRepository<GRNItem>, IGRNItemRepository
    {
        public GRNItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}