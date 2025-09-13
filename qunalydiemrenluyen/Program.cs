using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Utilities; // Thêm dòng này để gọi Functions
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Đảm bảo namespace này có để dùng IHttpContextAccessor
using Microsoft.Extensions.FileProviders; // Thêm namespace này để dùng PhysicalFileProvider

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Thêm kết nối Database vào dịch vụ
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session
builder.Services.AddDistributedMemoryCache(); // Cấu hình bộ nhớ cache để lưu session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true;  // Đảm bảo cookie chỉ có thể được truy cập từ HTTP
    options.Cookie.IsEssential = true;  // Cookie là thiết yếu để ứng dụng hoạt động
});

// ⚠️ Thêm dòng này để inject HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Thêm cấu hình CORS để cho phép truy cập từ Google Docs Viewer hoặc Office Online Viewer
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowExternalViewers", builder =>
    {
        builder.WithOrigins("https://docs.google.com", "https://view.officeapps.live.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // Cho phép gửi cookie nếu cần
    });
    // Thêm chính sách CORS cho môi trường phát triển (localhost)
    options.AddPolicy("AllowLocalDevelopment", builder =>
    {
        builder.WithOrigins("http://localhost:5053") // Thay 5053 bằng cổng của bạn nếu khác
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2️⃣ Cấu hình middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cấu hình phục vụ file tĩnh
app.UseStaticFiles(); // Phục vụ file từ wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads")),
    RequestPath = "/uploads" // Ánh xạ URL /uploads tới thư mục wwwroot/uploads
});

app.UseRouting();
app.UseSession();
// Kích hoạt CORS - Sử dụng chính sách phù hợp với môi trường
app.UseCors(builder => builder
    .WithOrigins(app.Environment.IsDevelopment() ? "http://localhost:5053" : "https://yourdomain.com")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseCors("AllowExternalViewers"); // Cho phép truy cập từ Google Docs và Office Online Viewer

// Kích hoạt sử dụng Session
app.UseStaticFiles();
app.UseAuthorization();

// ✅ Khởi tạo Functions với HttpContextAccessor
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
Functions.Configure(httpContextAccessor);

// 3️⃣ Cấu hình routing cho controller và areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Auth}/{action=Login}/{id?}");

app.Run();