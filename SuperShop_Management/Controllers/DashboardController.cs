using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Dashboard;
using SuperShop_Management.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IBatchRepository _batchRepo;
        private readonly IDepartmentRepository _departmentRepo;
        private readonly IPurchaseOrderRepository _poRepo;
        private readonly IInventoryRepository _inventoryRepo;

        public DashboardController(
            IProductRepository productRepo,
            ISupplierRepository supplierRepo,
            IBatchRepository batchRepo,
            IDepartmentRepository departmentRepo,
            IPurchaseOrderRepository poRepo,
            IInventoryRepository inventoryRepo)
        {
            _productRepo = productRepo;
            _supplierRepo = supplierRepo;
            _batchRepo = batchRepo;
            _departmentRepo = departmentRepo;
            _poRepo = poRepo;
            _inventoryRepo = inventoryRepo;
        }

        [HttpGet("stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStats()
        {
            var totalProducts = await _productRepo.CountAsync(p => p.IsActive);
            var totalSuppliers = await _supplierRepo.CountAsync(s => s.IsActive);
            var totalBatches = await _batchRepo.CountAsync(b => b.IsActive);
            var totalDepartments = await _departmentRepo.CountAsync(d => d.IsActive);
            var totalPurchaseOrders = await _poRepo.CountAsync(po => po.IsActive);

            var lowStock = await _inventoryRepo.GetLowStockItemsAsync();
            var lowStockCount = lowStock.Count();

            var recentPOs = await _poRepo.GetAllAsync();
            var recentActivities = recentPOs
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.OrderDate)
                .Take(5)
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Message = $"Purchase Order {p.PONumber} created.",
                    Time = GetRelativeTime(p.OrderDate),
                    Status = p.Status
                })
                .ToList();

            if (recentActivities.Count == 0)
            {
                recentActivities.Add(new RecentActivityDto { Id = 1, Message = "SuperShop Management System initialized.", Time = "1 hour ago", Status = "System" });
                recentActivities.Add(new RecentActivityDto { Id = 2, Message = "Initial product catalog imported successfully.", Time = "45 mins ago", Status = "Completed" });
                recentActivities.Add(new RecentActivityDto { Id = 3, Message = "Supplier network database updated.", Time = "12 mins ago", Status = "System" });
            }

            var stats = new DashboardResponseDto
            {
                TotalProducts = totalProducts,
                TotalSuppliers = totalSuppliers,
                TotalBatches = totalBatches,
                TotalDepartments = totalDepartments,
                TotalPurchaseOrders = totalPurchaseOrders,
                LowStockItems = lowStockCount,
                RecentActivities = recentActivities
            };

            return Ok(stats);
        }

        private static string GetRelativeTime(DateTime date)
        {
            var ts = DateTime.Now - date;
            if (ts.TotalMinutes < 1)
                return "Just now";
            if (ts.TotalMinutes < 60)
                return $"{(int)ts.TotalMinutes} mins ago";
            if (ts.TotalHours < 24)
                return $"{(int)ts.TotalHours} hours ago";
            return date.ToString("MMM dd, yyyy");
        }
    }
}
