using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm dịch vụ Controllers và Views
builder.Services.AddControllersWithViews();

// 2. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. QUAN TRỌNG: Đăng ký HttpContextAccessor (Dòng này sửa lỗi bạn đang gặp)
builder.Services.AddHttpContextAccessor();

// 4. Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Cấu hình Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 5. Kích hoạt Session (Đặt trước UseRouting)
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();