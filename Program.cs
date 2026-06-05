using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using untitled1.Data;
using untitled1.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Filmix API",
        Version = "v1",
        Description = "REST API for Filmix.\n\n" +
                      "**Cart endpoints** (`/api/cart`): open access.\n\n" +
                      "**Admin product endpoints** (`/api/admin/products`): require Admin role — " +
                      "log in at `/Account/Auth` as an admin before testing here."
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbProvider = builder.Configuration["DbProvider"] ?? "MySql";

if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Auth";
});

var app = builder.Build();

// Auto-migrate/create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        db.Database.EnsureCreated();
        // Only check core movie tables — failure here means the seed schema is stale and a full wipe is safe.
        // Identity (Users) and Orders tables are NOT checked here because wiping them would destroy
        // registered user accounts whenever a new feature table is added.
        _ = db.Movies.FirstOrDefault();
        _ = db.Episodes.FirstOrDefault();
        _ = db.MovieImages.FirstOrDefault();
    }
    catch (Exception ex)
    {
        logger.LogWarning("Phát hiện database cũ hoặc lỗi cấu trúc bảng: {Message}", ex.Message);
        try
        {
            logger.LogWarning("Đang tiến hành tự động xóa và khởi tạo lại database cùng dữ liệu mẫu mới...");
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            logger.LogInformation("Khởi tạo lại database và nạp dữ liệu mẫu mới thành công!");
        }
        catch (Exception dbEx)
        {
            logger.LogError(dbEx, "=========================================================================\n" +
                                "ERROR: Không thể kết nối hoặc khởi tạo cơ sở dữ liệu!\n" +
                                "Vui lòng đảm bảo dịch vụ MySQL/SQL Server đang chạy và thông tin kết nối chính xác.\n" +
                                "=========================================================================");
        }
    }
}

// Seed roles and admin accounts
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DbSeeder.SeedAsync(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Filmix Cart API v1");
    options.RoutePrefix = "swagger";
});

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
