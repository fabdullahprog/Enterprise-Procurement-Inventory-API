using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SuperShop_Management.Data;
using SuperShop_Management.Repositories.Implementations;
using SuperShop_Management.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Services (Fixed - IdentityUser<int>, IdentityRole<int>)
builder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
                   .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 5. Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUnitSetRepository, UnitSetRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// MODULE 1: Employee Requisition
builder.Services.AddScoped<IEmployeeRequisitionRepository, EmployeeRequisitionRepository>();

// MODULE 2: Store Issue
builder.Services.AddScoped<IStoreIssueRepository, StoreIssueRepository>();

// MODULE 3: Purchase Requisition & RFQ
builder.Services.AddScoped<IRequisitionRepository, RequisitionRepository>();
builder.Services.AddScoped<IRFQRepository, RFQRepository>();
builder.Services.AddScoped<IRFQSupplierRepository, RFQSupplierRepository>();

// MODULE 4: Supplier Quotation
builder.Services.AddScoped<ISupplierQuotationRepository, SupplierQuotationRepository>();
builder.Services.AddScoped<IQuotationItemRepository, QuotationItemRepository>();

// MODULE 5: Comparative Statement
builder.Services.AddScoped<ICSRepository, CSRepository>();
builder.Services.AddScoped<ICSSupplierRowRepository, CSSupplierRowRepository>();

// MODULE 6: Purchase Order
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IPOItemRepository, POItemRepository>();

// MODULE 7: GRN
builder.Services.AddScoped<IGRNRepository, GRNRepository>();
builder.Services.AddScoped<IGRNItemRepository, GRNItemRepository>();

// Inventory Management
builder.Services.AddScoped<IBatchRepository, BatchRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();

// Location Management
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IFloorRepository, FloorRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<IAisleRepository, AisleRepository>();
builder.Services.AddScoped<IRackRepository, RackRepository>();
builder.Services.AddScoped<IShelfRepository, ShelfRepository>();
builder.Services.AddScoped<IBinRepository, BinRepository>();



// 6. Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SuperShop API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ========== Seed Data ==========
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<int>>>();

        Console.WriteLine("\n═════════════════════════════════════════");
        Console.WriteLine("DATABASE INITIALIZATION STARTING...");
        Console.WriteLine("═════════════════════════════════════════\n");

        // Run migrations
        Console.WriteLine("Running migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✓ Migrations completed successfully\n");

        // Seed Identity data
        Console.WriteLine("Seeding Identity data (Roles & Users)...");
        await SeedData.InitializeIdentityAsync(roleManager, userManager);
        Console.WriteLine("✓ Identity data seeded successfully\n");

        // Seed application data
        Console.WriteLine("Seeding application data...");
        await SeedData.InitializeAsync(dbContext);
    }
}
catch (Exception ex)
{
    Console.WriteLine("\n═════════════════════════════════════════");
    Console.WriteLine("✗ FATAL ERROR DURING INITIALIZATION");
    Console.WriteLine("═════════════════════════════════════════");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"\nInner Exception: {ex.InnerException.Message}");
        Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
    }
    Console.WriteLine("═════════════════════════════════════════\n");
    
    // Re-throw to prevent app from starting if critical error
    throw;
}

app.Run();

