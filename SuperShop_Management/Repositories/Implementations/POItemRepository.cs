using SuperShop_Management.Data;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class POItemRepository : GenericRepository<POItem>, IPOItemRepository
    {
        public POItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}