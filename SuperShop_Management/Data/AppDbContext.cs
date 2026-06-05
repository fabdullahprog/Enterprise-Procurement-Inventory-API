using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Entities;
using SuperShop_Management.Entities.Location;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<UnitSet> UnitSets { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        
        // MODULE 1: Employee Requisition
        public DbSet<Requisition> Requisitions { get; set; }
        public DbSet<EmployeeRequisitionItem> EmployeeRequisitionItems { get; set; }
        
        // MODULE 2: Store Issue
        public DbSet<StoreIssue> StoreIssues { get; set; }
        
        // MODULE 3: Purchase Requisition & RFQ
        public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
        public DbSet<RequisitionItem> RequisitionItems { get; set; }
        public DbSet<RequestForQuotation> RequestForQuotations { get; set; }
        public DbSet<RFQSupplier> RFQSuppliers { get; set; }
        
        // MODULE 4: Supplier Quotation
        public DbSet<SupplierQuotation> SupplierQuotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        
        // MODULE 5: Comparative Statement
        public DbSet<ComparativeStatement> ComparativeStatements { get; set; }
        public DbSet<CSItem> CSItems { get; set; }
        public DbSet<CSSupplierRow> CSSupplierRows { get; set; }
        
        // MODULE 6: Purchase Order
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<POItem> POItems { get; set; }
        
        // MODULE 7: GRN
        public DbSet<GRN> GRNs { get; set; }
        public DbSet<GRNItem> GRNItems { get; set; }
        public DbSet<QualityCheck> QualityChecks { get; set; }
        public DbSet<QCItem> QCItems { get; set; }
        
        // Inventory Management
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        // Location Hierarchy
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Aisle> Aisles { get; set; }
        public DbSet<Rack> Racks { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<Bin> Bins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== Unique Constraints ==========
            modelBuilder.Entity<ItemCategory>()
                .HasIndex(i => i.CategoryName)
                .IsUnique();

            // ========== Decimal Precision ==========
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.UnitPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.TotalPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.BDTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SupplierQuotation>()
                .Property(sq => sq.TotalBDTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ComparativeStatement>()
                .Property(cs => cs.TotalBDTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.TotalBDTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<POItem>()
                .Property(pi => pi.SupplierRate)
                .HasPrecision(18, 2);
            modelBuilder.Entity<POItem>()
                .Property(pi => pi.PORate)
                .HasPrecision(18, 2);
            modelBuilder.Entity<POItem>()
                .Property(pi => pi.TotalPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<POItem>()
                .Property(pi => pi.BDTAmount)
                .HasPrecision(18, 2);

            // ========== Fix Multiple Cascade Paths – সব Foreign Key-তে Restrict ==========

            // --- Product & Unit (shadow property সমাধান) ---
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Unit)
                .WithMany()
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Unit>()
                .HasOne(u => u.UnitSet)
                .WithMany()
                .HasForeignKey(u => u.UnitSetId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- CSItem ---
            modelBuilder.Entity<CSItem>()
                .HasOne(c => c.ComparativeStatement)
                .WithMany(cs => cs.CSItems)
                .HasForeignKey(c => c.ComparativeStatementId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CSItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // OLD structure support (backward compatibility)
            modelBuilder.Entity<CSItem>()
                .HasOne(c => c.SelectedQuotationItem)
                .WithMany()
                .HasForeignKey(c => c.SelectedQuotationItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- ComparativeStatement ---
            modelBuilder.Entity<ComparativeStatement>()
                .HasOne(cs => cs.RFQ)
                .WithMany()
                .HasForeignKey(cs => cs.RFQId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComparativeStatement>()
                .HasOne(cs => cs.CreatedByUser)
                .WithMany()
                .HasForeignKey(cs => cs.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComparativeStatement>()
                .HasOne(cs => cs.ApprovedBy)
                .WithMany()
                .HasForeignKey(cs => cs.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // --- SupplierQuotation ---
            modelBuilder.Entity<SupplierQuotation>()
                .HasOne(sq => sq.RFQ)
                .WithMany(r => r.SupplierQuotations)
                .HasForeignKey(sq => sq.RFQId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierQuotation>()
                .HasOne(sq => sq.Supplier)
                .WithMany()
                .HasForeignKey(sq => sq.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierQuotation>()
                .HasOne(sq => sq.Currency)
                .WithMany()
                .HasForeignKey(sq => sq.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- QuotationItem ---
            modelBuilder.Entity<QuotationItem>()
                .HasOne(qi => qi.SupplierQuotation)
                .WithMany(sq => sq.QuotationItems)
                .HasForeignKey(qi => qi.SupplierQuotationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuotationItem>()
                .HasOne(qi => qi.Product)
                .WithMany()
                .HasForeignKey(qi => qi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- RequestForQuotation ---
            modelBuilder.Entity<RequestForQuotation>()
                .HasOne(r => r.Requisition)
                .WithMany(req => req.RequestForQuotations)
                .HasForeignKey(r => r.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestForQuotation>()
                .HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // --- PurchaseRequisition & RequisitionItem ---
            modelBuilder.Entity<PurchaseRequisition>()
                .ToTable("Requisitions");  // Explicitly set table name

            modelBuilder.Entity<PurchaseRequisition>()
                .HasOne(pr => pr.Department)
                .WithMany()
                .HasForeignKey(pr => pr.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseRequisition>()
                .HasOne(pr => pr.RequestedBy)
                .WithMany()
                .HasForeignKey(pr => pr.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseRequisition>()
                .HasOne(pr => pr.ApprovedBy)
                .WithMany()
                .HasForeignKey(pr => pr.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequisitionItem>()
                .HasOne(ri => ri.Requisition)
                .WithMany(r => r.RequisitionItems)
                .HasForeignKey(ri => ri.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequisitionItem>()
                .HasOne(ri => ri.Product)
                .WithMany()
                .HasForeignKey(ri => ri.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Product with Category, SubCategory, Brand (Unit আগেই করা হয়েছে) ---
            modelBuilder.Entity<Product>()
                .HasOne(p => p.ItemCategory)
                .WithMany()
                .HasForeignKey(p => p.ItemCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany()
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Brand, SubCategory, ItemCategory ---
            modelBuilder.Entity<Brand>()
                .HasOne(b => b.SubCategory)
                .WithMany(sc => sc.Brands)
                .HasForeignKey(b => b.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubCategory>()
                .HasOne(sc => sc.ItemCategory)
                .WithMany(ic => ic.SubCategories)
                .HasForeignKey(sc => sc.ItemCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Supplier, Currency ---
            modelBuilder.Entity<Supplier>()
                .HasOne(s => s.Currency)
                .WithMany()
                .HasForeignKey(s => s.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== PurchaseOrder & POItem ==========
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany()
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.ComparativeStatement)
                .WithMany(cs => cs.PurchaseOrders)
                .HasForeignKey(po => po.ComparativeStatementId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.CreatedByUser)
                .WithMany()
                .HasForeignKey(po => po.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.ApprovedBy)
                .WithMany()
                .HasForeignKey(po => po.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POItem>()
                .HasOne(pi => pi.PurchaseOrder)
                .WithMany(po => po.POItems)
                .HasForeignKey(pi => pi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POItem>()
                .HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== GRN, GRNItem, QualityCheck, QCItem ==========
            modelBuilder.Entity<GRN>()
                .HasOne(g => g.PurchaseOrder)
                .WithMany(po => po.GRNs)
                .HasForeignKey(g => g.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GRN>()
                .HasOne(g => g.ReceivedBy)
                .WithMany()
                .HasForeignKey(g => g.ReceivedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GRNItem>()
                .HasOne(gi => gi.GRN)
                .WithMany(g => g.GRNItems)
                .HasForeignKey(gi => gi.GRNId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GRNItem>()
                .HasOne(gi => gi.POItem)
                .WithMany()
                .HasForeignKey(gi => gi.POItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QualityCheck>()
                .HasOne(qc => qc.GRN)
                .WithOne(g => g.QualityCheck)
                .HasForeignKey<QualityCheck>(qc => qc.GRNId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QualityCheck>()
                .HasOne(qc => qc.InspectedBy)
                .WithMany()
                .HasForeignKey(qc => qc.InspectedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QCItem>()
                .HasOne(qci => qci.QualityCheck)
                .WithMany(qc => qc.QCItems)
                .HasForeignKey(qci => qci.QualityCheckId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QCItem>()
                .HasOne(qci => qci.GRNItem)
                .WithMany()
                .HasForeignKey(qci => qci.GRNItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== Batch, Inventory, StockMovement (New) ==========
            // Batch relationships
            modelBuilder.Entity<Batch>()
                .HasOne(b => b.Product)
                .WithMany(p => p.Batches)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Batch>()
                .HasOne(b => b.Supplier)
                .WithMany()
                .HasForeignKey(b => b.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Batch>()
                .HasOne(b => b.GRN)
                .WithMany()
                .HasForeignKey(b => b.GRNId)
                .OnDelete(DeleteBehavior.Restrict);

            // Inventory relationships
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Batch)
                .WithMany(b => b.Inventories)
                .HasForeignKey(i => i.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.GRN)
                .WithMany()
                .HasForeignKey(i => i.GRNId)
                .OnDelete(DeleteBehavior.Restrict);

            // StockMovement relationships
            modelBuilder.Entity<StockMovement>()
                .HasOne(s => s.Inventory)
                .WithMany(i => i.Movements)
                .HasForeignKey(s => s.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // StockMovement Location Chain (Bin level navigation)
            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.FromBin)
                .WithMany(b => b.OutgoingMovements)
                .HasForeignKey(sm => sm.FromBinId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.ToBin)
                .WithMany(b => b.IncomingMovements)
                .HasForeignKey(sm => sm.ToBinId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== MODULE 1: Requisition (Employee-level) ==========
            modelBuilder.Entity<Requisition>()
                .ToTable("EmployeeRequisitions");  // Different table name

            modelBuilder.Entity<Requisition>()
                .HasOne(r => r.RequestedByUser)
                .WithMany()
                .HasForeignKey(r => r.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Requisition>()
                .HasOne(r => r.Department)
                .WithMany()
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // EmployeeRequisitionItem relationships
            modelBuilder.Entity<EmployeeRequisitionItem>()
                .HasOne(eri => eri.EmployeeRequisition)
                .WithMany(r => r.Items)
                .HasForeignKey(eri => eri.EmployeeRequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeRequisitionItem>()
                .HasOne(eri => eri.Item)
                .WithMany()
                .HasForeignKey(eri => eri.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== MODULE 2: Store Issue ==========
            modelBuilder.Entity<StoreIssue>()
                .HasOne(si => si.Requisition)
                .WithMany(r => r.StoreIssues)
                .HasForeignKey(si => si.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StoreIssue>()
                .HasOne(si => si.IssuedBy)
                .WithMany()
                .HasForeignKey(si => si.IssuedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== MODULE 3: RFQ Suppliers ==========
            modelBuilder.Entity<RFQSupplier>()
                .HasOne(rs => rs.RFQ)
                .WithMany()
                .HasForeignKey(rs => rs.RFQId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RFQSupplier>()
                .HasOne(rs => rs.Supplier)
                .WithMany()
                .HasForeignKey(rs => rs.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== MODULE 5: CS Supplier Rows ==========
            modelBuilder.Entity<CSSupplierRow>()
                .HasOne(csr => csr.CS)
                .WithMany()
                .HasForeignKey(csr => csr.CSId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CSSupplierRow>()
                .HasOne(csr => csr.CSItem)
                .WithMany(ci => ci.SupplierRows)
                .HasForeignKey(csr => csr.CSItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CSSupplierRow>()
                .HasOne(csr => csr.Supplier)
                .WithMany()
                .HasForeignKey(csr => csr.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CSSupplierRow>()
                .HasOne(csr => csr.QuotationItem)
                .WithMany()
                .HasForeignKey(csr => csr.QuotationItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== MODULE 6: PO MD Approval ==========
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.MDApprovedBy)
                .WithMany()
                .HasForeignKey(po => po.MDApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== MODULE 7: GRN Store Approval ==========
            modelBuilder.Entity<GRN>()
                .HasOne(g => g.StoreApprovedBy)
                .WithMany()
                .HasForeignKey(g => g.StoreApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== Location Hierarchy ==========
            modelBuilder.Entity<Floor>().HasOne(f => f.Warehouse).WithMany().HasForeignKey(f => f.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Zone>().HasOne(z => z.Floor).WithMany().HasForeignKey(z => z.FloorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Aisle>().HasOne(a => a.Zone).WithMany().HasForeignKey(a => a.ZoneId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rack>().HasOne(r => r.Aisle).WithMany().HasForeignKey(r => r.AisleId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Shelf>().HasOne(s => s.Rack).WithMany().HasForeignKey(s => s.RackId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Bin>().HasOne(b => b.Shelf).WithMany().HasForeignKey(b => b.ShelfId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}