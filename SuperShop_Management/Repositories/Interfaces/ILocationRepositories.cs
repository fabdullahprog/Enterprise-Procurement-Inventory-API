using SuperShop_Management.Entities.Location;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Repositories.Interfaces
{
    public interface IWarehouseRepository : IGenericRepository<Warehouse> { }

    public interface IFloorRepository : IGenericRepository<Floor> 
    {
        Task<IEnumerable<Floor>> GetAllWithParentsAsync();
    }

    public interface IZoneRepository : IGenericRepository<Zone> 
    {
        Task<IEnumerable<Zone>> GetAllWithParentsAsync();
    }

    public interface IAisleRepository : IGenericRepository<Aisle> 
    {
        Task<IEnumerable<Aisle>> GetAllWithParentsAsync();
    }

    public interface IRackRepository : IGenericRepository<Rack> 
    {
        Task<IEnumerable<Rack>> GetAllWithParentsAsync();
    }

    public interface IShelfRepository : IGenericRepository<Shelf> 
    {
        Task<IEnumerable<Shelf>> GetAllWithParentsAsync();
    }

    public interface IBinRepository : IGenericRepository<Bin> 
    {
        Task<IEnumerable<Bin>> GetAllWithParentsAsync();
    }
}