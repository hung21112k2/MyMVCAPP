using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Thêm services vào container
builder.Services.AddControllersWithViews();

// Cấu hình DbContext với MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            mysqlOptions.CommandTimeout(60);
        });
    options.UseLoggerFactory(LoggerFactory.Create(b => b
        .AddConsole()
        .AddFilter(level => level >= LogLevel.Information)));
});

// Cấu hình session (nếu cần)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed dữ liệu ban đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Tự động áp dụng migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        // Khởi tạo dữ liệu mẫu
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo database");
    }
}

// Cấu hình pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Kích hoạt session

app.UseAuthorization();

// Cấu hình route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();

// Class để seed dữ liệu
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        if (!context.Products.Any())
        {
            await context.Products.AddRangeAsync(
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Price = 27990000,
                    Description = "Điện thoại cao cấp của Apple",
                    CreatedDate = DateTime.Now.AddDays(-10)
                },
                new Product
                {
                    Name = "Samsung Galaxy S23 Ultra",
                    Price = 24990000,
                    Description = "Flagship của Samsung với camera 200MP",
                    CreatedDate = DateTime.Now.AddDays(-5)
                },
                new Product
                {
                    Name = "MacBook Pro M2",
                    Price = 42990000,
                    Description = "Laptop chuyên nghiệp cho công việc sáng tạo",
                    CreatedDate = DateTime.Now.AddDays(-2)
                });

            await context.SaveChangesAsync();
        }
    }
}