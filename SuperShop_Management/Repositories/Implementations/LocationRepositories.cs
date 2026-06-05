using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.Entities.Location;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Repositories.Implementations
{
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(AppDbContext context) : base(context) { }
    }

    public class FloorRepository : GenericRepository<Floor>, IFloorRepository
    {
        private readonly AppDbContext _context;
        public FloorRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<Floor>> GetAllWithParentsAsync()
        {
            return await _context.Floors
                .Include(f => f.Warehouse)
                .Where(f => f.IsActive)
                .ToListAsync();
        }
    }

    public class ZoneRepository : GenericRepository<Zone>, IZoneRepository
    {
        private readonly AppDbContext _context;
        public ZoneRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<Zone>> GetAllWithParentsAsync()
        {
            return await _context.Zones
                .Include(z => z.Floor).ThenInclude(f => f!.Warehouse)
                .Include(z => z.Warehouse)
                .Where(z => z.IsActive)
                .ToListAsync();
        }
    }

    public class AisleRepository : GenericRepository<Aisle>, IAisleRepository
    {
        private readonly AppDbContext _context;
        public AisleRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<Aisle>> GetAllWithParentsAsync()
        {
            return await _context.Aisles
                .Include(a => a.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(a => a.Floor).ThenInclude(f => f!.Warehouse)
                .Include(a => a.Warehouse)
                .Where(a => a.IsActive)
                .ToListAsync();
        }
    }

    public class RackRepository : GenericRepository<Rack>, IRackRepository
    {
        private readonly AppDbContext _context;
        public RackRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<Rack>> GetAllWithParentsAsync()
        {
            return await _context.Racks
                .Include(r => r.Aisle).ThenInclude(a => a!.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(r => r.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(r => r.Floor).ThenInclude(f => f!.Warehouse)
                .Include(r => r.Warehouse)
                .Where(r => r.IsActive)
                .ToListAsync();
        }
    }

    public class ShelfRepository : GenericRepository<Shelf>, IShelfRepository
    {
        private readonly AppDbContext _context;
        public ShelfRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<Shelf>> GetAllWithParentsAsync()
        {
            return await _context.Shelves
                .Include(s => s.Rack).ThenInclude(r => r!.Aisle).ThenInclude(a => a!.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(s => s.Aisle).ThenInclude(a => a!.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(s => s.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(s => s.Floor).ThenInclude(f => f!.Warehouse)
                .Include(s => s.Warehouse)
                .Where(s => s.IsActive)
                .ToListAsync();
        }
    }

    public class BinRepository : GenericRepository<Bin>, IBinRepository
    {
        private readonly AppDbContext _context;
        public BinRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<Bin>> GetAllWithParentsAsync()
        {
            return await _context.Bins
                .Include(b => b.Shelf).ThenInclude(s => s!.Rack).ThenInclude(r => r!.Aisle).ThenInclude(a => a!.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(b => b.Rack).ThenInclude(r => r!.Aisle).ThenInclude(a => a!.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(b => b.Aisle).ThenInclude(a => a!.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(b => b.Zone).ThenInclude(z => z!.Floor).ThenInclude(f => f!.Warehouse)
                .Include(b => b.Floor).ThenInclude(f => f!.Warehouse)
                .Include(b => b.Warehouse)
                .Where(b => b.IsActive)
                .ToListAsync();
        }
    }
}