using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;

namespace SuperShop_Management.Data
{
    public static class SeedData
    {
        public static async Task InitializeIdentityAsync(
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<IdentityUser<int>> userManager)
        {
            // 1. Create Roles
            // These are used by the frontend dashboards and admin tools.
            string[] roles = { 
                "Admin", 
                "Manager", 
                "User", 
                "Employee",
                "PurchaseOfficer", 
                "PurchaseManager",
                "StoreManager", 
                "WarehouseManager",
                "DepartmentHead",
                "MD"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                    Console.WriteLine($"✓ Role created: {role}");
                }
            }

            // 2. Assign Default Permissions to Roles
            await AssignDefaultPermissionsAsync(roleManager);

            // 3. Create Admin User
            var adminEmail = "admin@supershop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser<int>
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("✓ Admin user created with Admin role");
                }
            }
            else
            {
                var isAdmin = await userManager.IsInRoleAsync(adminUser, "Admin");
                if (!isAdmin)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("✓ Admin role added to existing admin user");
                }
            }
        }

        private static async Task AssignDefaultPermissionsAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            const string PermissionClaimType = "permission";

            // Define default permissions for each role
            var rolePermissions = new Dictionary<string, List<string>>
            {
                ["Admin"] = new List<string>
                {
                    "requisition:create",
                    "requisition:view",
                    "requisition:approve",
                    "requisition:cancel",
                    "requisition:manage",
                    "role:manage",
                    "user:manage",
                    "department:manage"
                },
                ["Manager"] = new List<string>
                {
                    "requisition:create",
                    "requisition:view",
                    "requisition:approve",
                    "requisition:manage"
                },
                ["DepartmentHead"] = new List<string>
                {
                    "requisition:create",
                    "requisition:view",
                    "requisition:approve",
                    "requisition:cancel"
                },
                ["PurchaseManager"] = new List<string>
                {
                    "requisition:view",
                    "requisition:approve",
                    "requisition:manage",
                    "rfq:create",
                    "rfq:manage",
                    "quotation:create",
                    "cs:create",
                    "po:create"
                },
                ["PurchaseOfficer"] = new List<string>
                {
                    "requisition:create",
                    "requisition:view",
                    "rfq:create",
                    "quotation:create",
                    "cs:create",
                    "po:create"
                },
                ["StoreManager"] = new List<string>
                {
                    "requisition:view",
                    "inventory:manage",
                    "stock:manage"
                },
                ["WarehouseManager"] = new List<string>
                {
                    "inventory:view",
                    "qc:manage",
                    "grn:manage"
                },
                ["MD"] = new List<string>
                {
                    "requisition:view",
                    "requisition:approve",
                    "cs:approve",
                    "po:approve"
                },
                ["Employee"] = new List<string>
                {
                    "requisition:create",
                    "requisition:view"
                },
                ["User"] = new List<string>
                {
                    "requisition:create",
                    "requisition:view"
                }
            };

            foreach (var (roleName, permissions) in rolePermissions)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var existingClaims = await roleManager.GetClaimsAsync(role);
                var existingPermissions = existingClaims
                    .Where(c => c.Type == PermissionClaimType)
                    .Select(c => c.Value)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var permission in permissions)
                {
                    if (!existingPermissions.Contains(permission))
                    {
                        await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(PermissionClaimType, permission));
                        Console.WriteLine($"  ✓ Permission '{permission}' added to role '{roleName}'");
                    }
                }
            }

            Console.WriteLine("✓ Default permissions assigned to roles");
        }

        public static async Task InitializeAsync(AppDbContext context)
        {
            try
            {
                // ─── 1. Currency ───────────────────────────────────────────────
                if (!context.Currencies.Any())
                {
                    try
                    {
                        var currencies = GetCurrencies();
                        await context.Currencies.AddRangeAsync(currencies);
                        int result = await context.SaveChangesAsync();
                        Console.WriteLine($"✓ Currency seeded: {result} records added");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Currency seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ Currency: Already seeded, skipping...");
                }

                // ─── 2. Department ─────────────────────────────────────────────
                if (!context.Departments.Any())
                {
                    try
                    {
                        var departments = GetDepartments();
                        await context.Departments.AddRangeAsync(departments);
                        int result = await context.SaveChangesAsync();
                        Console.WriteLine($"✓ Department seeded: {result} records added");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Department seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ Department: Already seeded, skipping...");
                }

                // ─── 3. UnitSet ────────────────────────────────────────────────
                if (!context.UnitSets.Any())
                {
                    try
                    {
                        var unitSets = GetUnitSets();
                        await context.UnitSets.AddRangeAsync(unitSets);
                        int result = await context.SaveChangesAsync();
                        Console.WriteLine($"✓ UnitSet seeded: {result} records added");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ UnitSet seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ UnitSet: Already seeded, skipping...");
                }

                // ─── 4. Unit (depends on UnitSet) ──────────────────────────────
                if (!context.Units.Any())
                {
                    try
                    {
                        var unitSets = context.UnitSets.ToList();
                        if (!unitSets.Any())
                        {
                            Console.WriteLine("✗ Unit seeding failed: No UnitSets found in database");
                        }
                        else
                        {
                            var units = GetUnits(unitSets);
                            await context.Units.AddRangeAsync(units);
                            int result = await context.SaveChangesAsync();
                            Console.WriteLine($"✓ Unit seeded: {result} records added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Unit seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ Unit: Already seeded, skipping...");
                }

                // ─── 5. ItemCategory ───────────────────────────────────────────
                if (!context.ItemCategories.Any())
                {
                    try
                    {
                        var categories = GetItemCategories();
                        await context.ItemCategories.AddRangeAsync(categories);
                        int result = await context.SaveChangesAsync();
                        Console.WriteLine($"✓ ItemCategory seeded: {result} records added");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ ItemCategory seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ ItemCategory: Already seeded, skipping...");
                }

                // ─── 6. SubCategory (depends on ItemCategory) ──────────────────
                if (!context.SubCategories.Any())
                {
                    try
                    {
                        var categories = context.ItemCategories.ToList();
                        if (!categories.Any())
                        {
                            Console.WriteLine("✗ SubCategory seeding failed: No ItemCategories found in database");
                        }
                        else
                        {
                            var subCategories = GetSubCategories(categories);
                            await context.SubCategories.AddRangeAsync(subCategories);
                            int result = await context.SaveChangesAsync();
                            Console.WriteLine($"✓ SubCategory seeded: {result} records added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ SubCategory seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ SubCategory: Already seeded, skipping...");
                }

                // ─── 7. Brand (depends on SubCategory) ─────────────────────────
                if (!context.Brands.Any())
                {
                    try
                    {
                        var subCategories = context.SubCategories.ToList();
                        if (!subCategories.Any())
                        {
                            Console.WriteLine("✗ Brand seeding failed: No SubCategories found in database");
                        }
                        else
                        {
                            var brands = GetBrands(subCategories);
                            await context.Brands.AddRangeAsync(brands);
                            int result = await context.SaveChangesAsync();
                            Console.WriteLine($"✓ Brand seeded: {result} records added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Brand seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ Brand: Already seeded, skipping...");
                }

                // ─── 8. Supplier (depends on Currency) ─────────────────────────
                if (!context.Suppliers.Any())
                {
                    try
                    {
                        var currencies = context.Currencies.ToList();
                        if (!currencies.Any())
                        {
                            Console.WriteLine("✗ Supplier seeding failed: No Currencies found in database");
                        }
                        else
                        {
                            var suppliers = GetSuppliers(currencies);
                            await context.Suppliers.AddRangeAsync(suppliers);
                            int result = await context.SaveChangesAsync();
                            Console.WriteLine($"✓ Supplier seeded: {result} records added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Supplier seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ Supplier: Already seeded, skipping...");
                }

                // ─── 9. Product (depends on ItemCategory, SubCategory, Brand, Unit) ───
                if (!context.Products.Any())
                {
                    try
                    {
                        var categories = context.ItemCategories.ToList();
                        var subCategories = context.SubCategories.ToList();
                        var brands = context.Brands.ToList();
                        var units = context.Units.ToList();

                        if (!categories.Any() || !subCategories.Any() || !brands.Any() || !units.Any())
                        {
                            Console.WriteLine("✗ Product seeding failed: Missing dependencies (Categories, SubCategories, Brands, or Units)");
                        }
                        else
                        {
                            var products = GetProducts(categories, subCategories, brands, units);
                            await context.Products.AddRangeAsync(products);
                            int result = await context.SaveChangesAsync();
                            Console.WriteLine($"✓ Product seeded: {result} records added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Product seeding failed: {ex.Message}");
                        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ Product: Already seeded, skipping...");
                }

                Console.WriteLine("\n═════════════════════════════════════════");
                Console.WriteLine("✓ Seed data initialization completed");
                Console.WriteLine("═════════════════════════════════════════\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ FATAL: Seed data initialization failed with exception:");
                Console.WriteLine($"  {ex.Message}");
                Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SEED DATA METHODS
        // ─────────────────────────────────────────────────────────────────────

        private static List<Currency> GetCurrencies()
        {
            return new List<Currency>
            {
                // ── Original 5 ──
                new() { Code = "BDT", Symbol = "৳", Name = "Bangladeshi Taka",  ExchangeRate = 1.000000m, IsBaseCurrency = true,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "USD", Symbol = "$", Name = "US Dollar",          ExchangeRate = 110.50m,   IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "EUR", Symbol = "€", Name = "Euro",               ExchangeRate = 120.25m,   IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "GBP", Symbol = "£", Name = "British Pound",      ExchangeRate = 140.75m,   IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "SAR", Symbol = "﷼", Name = "Saudi Riyal",        ExchangeRate = 29.40m,    IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { Code = "INR", Symbol = "₹", Name = "Indian Rupee",       ExchangeRate = 1.32m,     IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "AED", Symbol = "د.إ", Name = "UAE Dirham",       ExchangeRate = 30.10m,    IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "MYR", Symbol = "RM", Name = "Malaysian Ringgit", ExchangeRate = 24.80m,    IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "SGD", Symbol = "S$", Name = "Singapore Dollar",  ExchangeRate = 82.60m,    IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Code = "JPY", Symbol = "¥", Name = "Japanese Yen",       ExchangeRate = 0.74m,     IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<Department> GetDepartments()
        {
            return new List<Department>
            {
                // ── Original 5 ──
                new() { DepartmentCode = "ADMIN",  DepartmentName = "Administration",      Description = "General administrative department",           DepartmentEmail = "admin@supershop.com",     DepartmentPhone = "01700000001", Location = "Head Office, Floor 1",    CanRequestItem = false, CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "STORE",  DepartmentName = "Store & Warehouse",   Description = "Manages inventory and stock",                  DepartmentEmail = "store@supershop.com",     DepartmentPhone = "01700000002", Location = "Warehouse, Ground Floor", CanRequestItem = false, CanIssueItem = true,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "PURCH",  DepartmentName = "Purchase",            Description = "Handles all procurement activities",           DepartmentEmail = "purchase@supershop.com",  DepartmentPhone = "01700000003", Location = "Head Office, Floor 2",    CanRequestItem = true,  CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "SALES",  DepartmentName = "Sales",               Description = "Manages sales operations and customer service", DepartmentEmail = "sales@supershop.com",     DepartmentPhone = "01700000004", Location = "Showroom, Ground Floor",  CanRequestItem = true,  CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "ACCTS",  DepartmentName = "Accounts & Finance",  Description = "Financial management and accounting",          DepartmentEmail = "accounts@supershop.com",  DepartmentPhone = "01700000005", Location = "Head Office, Floor 3",    CanRequestItem = false, CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { DepartmentCode = "HR",     DepartmentName = "Human Resources",     Description = "Recruitment, payroll and employee management",  DepartmentEmail = "hr@supershop.com",        DepartmentPhone = "01700000006", Location = "Head Office, Floor 2",    CanRequestItem = true,  CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "IT",     DepartmentName = "IT & Technology",     Description = "Manages IT infrastructure and software",        DepartmentEmail = "it@supershop.com",        DepartmentPhone = "01700000007", Location = "Head Office, Floor 4",    CanRequestItem = true,  CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "MKTG",   DepartmentName = "Marketing",           Description = "Promotions, branding and advertising",          DepartmentEmail = "marketing@supershop.com", DepartmentPhone = "01700000008", Location = "Head Office, Floor 1",    CanRequestItem = true,  CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "LOGST",  DepartmentName = "Logistics",           Description = "Transportation and delivery management",         DepartmentEmail = "logistics@supershop.com", DepartmentPhone = "01700000009", Location = "Warehouse, Floor 1",      CanRequestItem = true,  CanIssueItem = true,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { DepartmentCode = "QC",     DepartmentName = "Quality Control",     Description = "Ensures product quality and compliance",         DepartmentEmail = "qc@supershop.com",        DepartmentPhone = "01700000010", Location = "Warehouse, Ground Floor", CanRequestItem = false, CanIssueItem = false, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<UnitSet> GetUnitSets()
        {
            return new List<UnitSet>
            {
                // ── Original 5 ──
                new() { NameOfUnitSet = "Weight",      Description = "Units based on weight measurement",    Remarks = "e.g. kg, g, mg",          IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Volume",      Description = "Units based on liquid volume",          Remarks = "e.g. L, mL",              IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Quantity",    Description = "Units based on countable items",        Remarks = "e.g. Piece, Dozen, Box",  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Length",      Description = "Units based on length measurement",    Remarks = "e.g. m, cm, inch",         IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Area",        Description = "Units based on surface area",           Remarks = "e.g. sqft, sqm",           IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { NameOfUnitSet = "Temperature", Description = "Units based on temperature scale",      Remarks = "e.g. °C, °F",              IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Time",        Description = "Units based on time measurement",       Remarks = "e.g. hr, min, sec",        IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Packaging",   Description = "Units based on packaging type",         Remarks = "e.g. Box, Carton, Pack",   IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Digital",     Description = "Units for digital storage",             Remarks = "e.g. MB, GB, TB",          IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnitSet = "Energy",      Description = "Units based on energy measurement",     Remarks = "e.g. kWh, Joule",          IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<Unit> GetUnits(List<UnitSet> unitSets)
        {
            var weightSet = unitSets.First(u => u.NameOfUnitSet == "Weight");
            var volumeSet = unitSets.First(u => u.NameOfUnitSet == "Volume");
            var quantitySet = unitSets.First(u => u.NameOfUnitSet == "Quantity");
            var lengthSet = unitSets.First(u => u.NameOfUnitSet == "Length");
            var areaSet = unitSets.First(u => u.NameOfUnitSet == "Area");
            var packagingSet = unitSets.First(u => u.NameOfUnitSet == "Packaging");

            return new List<Unit>
            {
                // ── Original 7 ──
                new() { NameOfUnit = "Kilogram (kg)", UnitSetId = weightSet.UnitSetId,    UnitFactor = 1,    IsBaseUnit = true,  Description = "Base unit of weight",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Gram (g)",      UnitSetId = weightSet.UnitSetId,    UnitFactor = 0.001,IsBaseUnit = false, Description = "1 g = 0.001 kg",         IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Litre (L)",     UnitSetId = volumeSet.UnitSetId,    UnitFactor = 1,    IsBaseUnit = true,  Description = "Base unit of volume",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Piece (Pcs)",   UnitSetId = quantitySet.UnitSetId,  UnitFactor = 1,    IsBaseUnit = true,  Description = "Single countable item",   IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Dozen",         UnitSetId = quantitySet.UnitSetId,  UnitFactor = 12,   IsBaseUnit = false, Description = "12 pieces per dozen",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Metre (m)",     UnitSetId = lengthSet.UnitSetId,    UnitFactor = 1,    IsBaseUnit = true,  Description = "Base unit of length",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Sq. Foot",      UnitSetId = areaSet.UnitSetId,      UnitFactor = 1,    IsBaseUnit = true,  Description = "Square foot area unit",   IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { NameOfUnit = "Milligram (mg)",UnitSetId = weightSet.UnitSetId,    UnitFactor = 0.000001, IsBaseUnit = false, Description = "1 mg = 0.000001 kg",  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Millilitre (mL)",UnitSetId = volumeSet.UnitSetId,   UnitFactor = 0.001,    IsBaseUnit = false, Description = "1 mL = 0.001 L",      IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Box",           UnitSetId = packagingSet.UnitSetId,  UnitFactor = 1,        IsBaseUnit = true,  Description = "Standard box unit",    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Carton",        UnitSetId = packagingSet.UnitSetId,  UnitFactor = 12,       IsBaseUnit = false, Description = "12 boxes per carton",  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { NameOfUnit = "Centimetre (cm)",UnitSetId = lengthSet.UnitSetId,   UnitFactor = 0.01,     IsBaseUnit = false, Description = "1 cm = 0.01 m",       IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<ItemCategory> GetItemCategories()
        {
            return new List<ItemCategory>
            {
                // ── Original 6 ──
                new() { CategoryName = "Food & Beverage", CategoryDescription = "All food and drink items",               IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Personal Care",   CategoryDescription = "Personal hygiene and care products",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Household",       CategoryDescription = "Household cleaning and maintenance",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Electronics",     CategoryDescription = "Electronic devices and accessories",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Baby Care",       CategoryDescription = "Baby food, diapers and care products",   IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Stationery",      CategoryDescription = "Office and school supplies",             IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { CategoryName = "Health & Medicine",  CategoryDescription = "OTC medicines, vitamins and supplements", IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Clothing & Apparel", CategoryDescription = "Ready-made garments and accessories",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Sports & Fitness",   CategoryDescription = "Sporting goods and fitness equipment",    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Toys & Games",       CategoryDescription = "Children toys, board games and puzzles",  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { CategoryName = "Pet Supplies",       CategoryDescription = "Food and accessories for pets",            IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<SubCategory> GetSubCategories(List<ItemCategory> categories)
        {
            var food = categories.First(c => c.CategoryName == "Food & Beverage");
            var personalCare = categories.First(c => c.CategoryName == "Personal Care");
            var household = categories.First(c => c.CategoryName == "Household");
            var electronics = categories.First(c => c.CategoryName == "Electronics");
            var babyCare = categories.First(c => c.CategoryName == "Baby Care");
            var health = categories.First(c => c.CategoryName == "Health & Medicine");
            var clothing = categories.First(c => c.CategoryName == "Clothing & Apparel");

            return new List<SubCategory>
            {
                // ── Original 7 ──
                new() { SubCategoryName = "Beverages",           Description = "Soft drinks, juices, water",          ItemCategoryId = food.ItemCategoryId,         IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Snacks & Biscuits",   Description = "Chips, cookies and biscuits",         ItemCategoryId = food.ItemCategoryId,         IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Shampoo & Hair Care", Description = "Shampoos, conditioners and oils",     ItemCategoryId = personalCare.ItemCategoryId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Skin Care",           Description = "Lotions, creams and face wash",       ItemCategoryId = personalCare.ItemCategoryId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Cleaning Supplies",   Description = "Detergents, disinfectants, mops",     ItemCategoryId = household.ItemCategoryId,    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Mobile Accessories",  Description = "Phone cases, chargers, cables",       ItemCategoryId = electronics.ItemCategoryId,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Baby Food",           Description = "Infant formula and baby cereals",     ItemCategoryId = babyCare.ItemCategoryId,     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { SubCategoryName = "Dairy Products",      Description = "Milk, cheese, butter and yoghurt",   ItemCategoryId = food.ItemCategoryId,         IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "OTC Medicine",        Description = "Over-the-counter tablets and syrups", ItemCategoryId = health.ItemCategoryId,       IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Vitamins & Supplements", Description = "Multivitamins and health supplements", ItemCategoryId = health.ItemCategoryId,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Men's Wear",          Description = "Shirts, trousers and casual wear",   ItemCategoryId = clothing.ItemCategoryId,     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { SubCategoryName = "Kitchen Appliances",  Description = "Blenders, rice cookers and ovens",   ItemCategoryId = household.ItemCategoryId,    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<Brand> GetBrands(List<SubCategory> subCategories)
        {
            var beverages = subCategories.First(s => s.SubCategoryName == "Beverages");
            var snacks = subCategories.First(s => s.SubCategoryName == "Snacks & Biscuits");
            var hairCare = subCategories.First(s => s.SubCategoryName == "Shampoo & Hair Care");
            var skinCare = subCategories.First(s => s.SubCategoryName == "Skin Care");
            var cleaning = subCategories.First(s => s.SubCategoryName == "Cleaning Supplies");
            var dairy = subCategories.First(s => s.SubCategoryName == "Dairy Products");
            var otc = subCategories.First(s => s.SubCategoryName == "OTC Medicine");

            return new List<Brand>
            {
                // ── Original 5 ──
                new() { BrandName = "Pran",    Description = "Popular Bangladeshi food & beverage brand",  Country = "Bangladesh", Website = "https://www.pranfoods.net",  SubCategoryId = beverages.SubCategoryId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Ruchi",   Description = "Well-known snack manufacturer in Bangladesh",Country = "Bangladesh", Website = "https://www.ruchifoods.com", SubCategoryId = snacks.SubCategoryId,    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Sunsilk", Description = "International hair care brand by Unilever",  Country = "UK",         Website = "https://www.sunsilk.com",    SubCategoryId = hairCare.SubCategoryId,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Pond's",  Description = "Skin care brand by Unilever",                Country = "USA",        Website = "https://www.ponds.com",      SubCategoryId = skinCare.SubCategoryId,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Dettol",  Description = "Antiseptic and cleaning products by Reckitt",Country = "UK",         Website = "https://www.dettol.com.bd",  SubCategoryId = cleaning.SubCategoryId,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { BrandName = "Aarong Dairy",  Description = "Premium dairy products by BRAC",           Country = "Bangladesh", Website = "https://www.aarongdairy.com",  SubCategoryId = dairy.SubCategoryId,   IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Ispahani",      Description = "Renowned tea and food brand in Bangladesh", Country = "Bangladesh", Website = "https://www.ispahani.com",     SubCategoryId = beverages.SubCategoryId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Square Pharma", Description = "Leading pharmaceutical brand in Bangladesh",Country = "Bangladesh", Website = "https://www.squarepharma.com.bd", SubCategoryId = otc.SubCategoryId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Bashundhara",   Description = "Multi-product brand including food & tissue",Country = "Bangladesh", Website = "https://www.bashundhara.com",  SubCategoryId = snacks.SubCategoryId,  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { BrandName = "Lifebuoy",      Description = "Hygiene soap and handwash by Unilever",     Country = "UK",         Website = "https://www.lifebuoy.com",    SubCategoryId = cleaning.SubCategoryId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<Supplier> GetSuppliers(List<Currency> currencies)
        {
            var bdt = currencies.First(c => c.Code == "BDT");
            var usd = currencies.First(c => c.Code == "USD");

            return new List<Supplier>
            {
                // ── Original 5 ──
                new() { Name = "Pran-RFL Group",       ContactPerson = "Md. Rafiqul Islam",   Phone = "01711000001", Email = "supply@pranrfl.com",      Address = "105 Bir Uttam C.R. Datta Rd, Dhaka", TradeLicenseNo = "TL-001-2020", TINNo = "TIN-001", BINNo = "BIN-001", BankName = "Dutch Bangla Bank",  BankAccountNo = "1001001001", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "Unilever Bangladesh",  ContactPerson = "Ms. Farida Khanam",   Phone = "01711000002", Email = "supply@unilever.com.bd",  Address = "Road 2, Gulshan 1, Dhaka",           TradeLicenseNo = "TL-002-2020", TINNo = "TIN-002", BINNo = "BIN-002", BankName = "Standard Chartered", BankAccountNo = "2002002002", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "Reckitt Bangladesh",   ContactPerson = "Mr. Karim Hossain",   Phone = "01711000003", Email = "supply@reckitt.com.bd",   Address = "Plot 19, Uttara, Dhaka",             TradeLicenseNo = "TL-003-2020", TINNo = "TIN-003", BINNo = "BIN-003", BankName = "HSBC Bangladesh",    BankAccountNo = "3003003003", CurrencyId = usd.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "ACI Limited",          ContactPerson = "Mr. Arif Chowdhury",  Phone = "01711000004", Email = "supply@aci.com.bd",       Address = "245 Tejgaon, Dhaka",                 TradeLicenseNo = "TL-004-2020", TINNo = "TIN-004", BINNo = "BIN-004", BankName = "Islami Bank",         BankAccountNo = "4004004004", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "Square Consumer",      ContactPerson = "Ms. Nasrin Akter",    Phone = "01711000005", Email = "supply@squarecp.com.bd",  Address = "Pabna, Rajshahi Division",           TradeLicenseNo = "TL-005-2020", TINNo = "TIN-005", BINNo = "BIN-005", BankName = "Sonali Bank",         BankAccountNo = "5005005005", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                // ── Extra 5 ──
                new() { Name = "Bashundhara Group",    ContactPerson = "Mr. Sabbir Ahmed",    Phone = "01711000006", Email = "supply@bashundhara.com",  Address = "13 Bashundhara R/A, Dhaka",          TradeLicenseNo = "TL-006-2021", TINNo = "TIN-006", BINNo = "BIN-006", BankName = "Eastern Bank",        BankAccountNo = "6006006006", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "Ispahani Limited",     ContactPerson = "Mr. Ziaul Hoque",     Phone = "01711000007", Email = "supply@ispahani.com",     Address = "Agrabad C/A, Chittagong",            TradeLicenseNo = "TL-007-2021", TINNo = "TIN-007", BINNo = "BIN-007", BankName = "Pubali Bank",          BankAccountNo = "7007007007", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "BRAC Dairy & Food",    ContactPerson = "Ms. Rehana Begum",    Phone = "01711000008", Email = "supply@bracdairy.com.bd", Address = "75 Mohakhali, Dhaka",                TradeLicenseNo = "TL-008-2021", TINNo = "TIN-008", BINNo = "BIN-008", BankName = "BRAC Bank",           BankAccountNo = "8008008008", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "Transcom Group",       ContactPerson = "Mr. Latifur Rahman",  Phone = "01711000009", Email = "supply@transcom.com.bd",  Address = "Gulshan Avenue, Dhaka",              TradeLicenseNo = "TL-009-2021", TINNo = "TIN-009", BINNo = "BIN-009", BankName = "City Bank",           BankAccountNo = "9009009009", CurrencyId = usd.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
                new() { Name = "Globe Pharma Ltd",     ContactPerson = "Mr. Tanvir Hasan",    Phone = "01711000010", Email = "supply@globepharma.com.bd",Address = "Tongi, Gazipur",                    TradeLicenseNo = "TL-010-2022", TINNo = "TIN-010", BINNo = "BIN-010", BankName = "Janata Bank",          BankAccountNo = "1010101010", CurrencyId = bdt.CurrencyId, IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now },
            };
        }

        private static List<Product> GetProducts(
            List<ItemCategory> categories,
            List<SubCategory> subCategories,
            List<Brand> brands,
            List<Unit> units)
        {
            var foodCat = categories.First(c => c.CategoryName == "Food & Beverage");
            var personalCat = categories.First(c => c.CategoryName == "Personal Care");
            var houseCat = categories.First(c => c.CategoryName == "Household");
            var healthCat = categories.First(c => c.CategoryName == "Health & Medicine");

            var bevSub = subCategories.First(s => s.SubCategoryName == "Beverages");
            var snackSub = subCategories.First(s => s.SubCategoryName == "Snacks & Biscuits");
            var hairSub = subCategories.First(s => s.SubCategoryName == "Shampoo & Hair Care");
            var skinSub = subCategories.First(s => s.SubCategoryName == "Skin Care");
            var cleanSub = subCategories.First(s => s.SubCategoryName == "Cleaning Supplies");
            var dairySub = subCategories.First(s => s.SubCategoryName == "Dairy Products");
            var otcSub = subCategories.First(s => s.SubCategoryName == "OTC Medicine");

            var pranBrand = brands.First(b => b.BrandName == "Pran");
            var ruchiBrand = brands.First(b => b.BrandName == "Ruchi");
            var sunsilkBrand = brands.First(b => b.BrandName == "Sunsilk");
            var pondsBrand = brands.First(b => b.BrandName == "Pond's");
            var dettolBrand = brands.First(b => b.BrandName == "Dettol");
            var aarongBrand = brands.First(b => b.BrandName == "Aarong Dairy");
            var ispBrand = brands.First(b => b.BrandName == "Ispahani");
            var sqPharmaBrand = brands.First(b => b.BrandName == "Square Pharma");
            var lifebuoyBrand = brands.First(b => b.BrandName == "Lifebuoy");
            var bashBrand = brands.First(b => b.BrandName == "Bashundhara");

            var litreUnit = units.First(u => u.NameOfUnit == "Litre (L)");
            var pcsUnit = units.First(u => u.NameOfUnit == "Piece (Pcs)");
            var gramUnit = units.First(u => u.NameOfUnit == "Gram (g)");
            var mlUnit = units.First(u => u.NameOfUnit == "Millilitre (mL)");
            var boxUnit = units.First(u => u.NameOfUnit == "Box");

            return new List<Product>
            {
                // ── Original 5 ──
                new() {
                    Name = "Pran Mango Juice 250ml",    Barcode = "8901234567001", Price = 25.00m,  CurrentStock = 500,
                    IsPerishable = true,  Description = "Refreshing mango flavoured drink by Pran",
                    ItemCategoryId = foodCat.ItemCategoryId,     SubCategoryId = bevSub.SubCategoryId,
                    BrandId = pranBrand.BrandId,   UnitId = pcsUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Ruchi Chanachur 200g",       Barcode = "8901234567002", Price = 30.00m,  CurrentStock = 300,
                    IsPerishable = true,  Description = "Spicy chanachur snack by Ruchi",
                    ItemCategoryId = foodCat.ItemCategoryId,     SubCategoryId = snackSub.SubCategoryId,
                    BrandId = ruchiBrand.BrandId,  UnitId = pcsUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Sunsilk Shampoo 200ml",      Barcode = "8901234567003", Price = 120.00m, CurrentStock = 200,
                    IsPerishable = false, Description = "Smooth & manageable hair shampoo",
                    ItemCategoryId = personalCat.ItemCategoryId, SubCategoryId = hairSub.SubCategoryId,
                    BrandId = sunsilkBrand.BrandId, UnitId = pcsUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Pond's Cold Cream 50g",      Barcode = "8901234567004", Price = 85.00m,  CurrentStock = 150,
                    IsPerishable = false, Description = "Moisturising cold cream for soft skin",
                    ItemCategoryId = personalCat.ItemCategoryId, SubCategoryId = skinSub.SubCategoryId,
                    BrandId = pondsBrand.BrandId,  UnitId = pcsUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Dettol Liquid Soap 200ml",   Barcode = "8901234567005", Price = 95.00m,  CurrentStock = 250,
                    IsPerishable = false, Description = "Antibacterial liquid handwash by Dettol",
                    ItemCategoryId = houseCat.ItemCategoryId,    SubCategoryId = cleanSub.SubCategoryId,
                    BrandId = dettolBrand.BrandId, UnitId = pcsUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                // ── Extra 5 ──
                new() {
                    Name = "Aarong Full Cream Milk 1L",  Barcode = "8901234567006", Price = 75.00m,  CurrentStock = 400,
                    IsPerishable = true,  Description = "Fresh full cream pasteurised milk by Aarong Dairy",
                    ItemCategoryId = foodCat.ItemCategoryId,     SubCategoryId = dairySub.SubCategoryId,
                    BrandId = aarongBrand.BrandId, UnitId = litreUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Ispahani Mirzapore Tea 200g",Barcode = "8901234567007", Price = 180.00m, CurrentStock = 320,
                    IsPerishable = false, Description = "Premium blended tea by Ispahani",
                    ItemCategoryId = foodCat.ItemCategoryId,     SubCategoryId = bevSub.SubCategoryId,
                    BrandId = ispBrand.BrandId,    UnitId = pcsUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Square Napa 500mg Tablet",   Barcode = "8901234567008", Price = 8.00m,   CurrentStock = 1000,
                    IsPerishable = false, Description = "Paracetamol tablet for fever and pain relief",
                    ItemCategoryId = healthCat.ItemCategoryId,   SubCategoryId = otcSub.SubCategoryId,
                    BrandId = sqPharmaBrand.BrandId, UnitId = boxUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Lifebuoy Total Soap 90g",    Barcode = "8901234567009", Price = 40.00m,  CurrentStock = 600,
                    IsPerishable = false, Description = "Antibacterial protection bar soap by Lifebuoy",
                    ItemCategoryId = houseCat.ItemCategoryId,    SubCategoryId = cleanSub.SubCategoryId,
                    BrandId = lifebuoyBrand.BrandId, UnitId = pcsUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
                new() {
                    Name = "Bashundhara Tissue Box 150pcs",Barcode="8901234567010", Price = 120.00m, CurrentStock = 350,
                    IsPerishable = false, Description = "Soft facial tissue box by Bashundhara",
                    ItemCategoryId = houseCat.ItemCategoryId,    SubCategoryId = cleanSub.SubCategoryId,
                    BrandId = bashBrand.BrandId,   UnitId = boxUnit.UnitId,
                    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.Now
                },
            };
        }
    }
}