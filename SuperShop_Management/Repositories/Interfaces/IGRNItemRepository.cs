using SuperShop_Management.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IGRNItemRepository : IGenericRepository<GRNItem>
    {
        // ভবিষ্যতে যদি GRNItem-এর জন্য কোনো এক্সট্রা মেথডের দরকার হয়, তবে এখানে যোগ করবেন।
        // যেমন: Task<IEnumerable<GRNItem>> GetByGRNIdAsync(int grnId);
    }
}