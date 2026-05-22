using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using untitled1.Data;
using untitled1.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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
    try
    {
        db.Database.EnsureCreated();
        // Verify all required tables exist (triggers recreate if schema is stale)
        _ = db.Episodes.FirstOrDefault();
        _ = db.MovieImages.FirstOrDefault();
        _ = db.Users.FirstOrDefault();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
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
