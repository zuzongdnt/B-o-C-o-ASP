using dienthoai.Models;
using dienthoai.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm các dịch vụ vào container (DI)
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<QuanLyDienThoaiZuzongContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient("GeminiAI", client =>
{
    client.Timeout = TimeSpan.FromSeconds(14);
});

// 2. Cấu hình Session
builder.Services.AddDistributedMemoryCache(); // QUAN TRỌNG: Phải có Cache để lưu Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Bắt buộc lưu cookie kể cả khi chưa đồng ý chính sách
});

var app = builder.Build();

// 3. Cấu hình HTTP request pipeline (Thứ tự rất quan trọng)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets(); // (.NET 9) Nếu bạn dùng .NET cũ hơn thì đổi thành app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // QUAN TRỌNG: Phải đặt SAU UseRouting và TRƯỚC UseAuthorization

app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Session.GetString("MaTaiKhoan")) &&
        context.Request.Cookies.TryGetValue("ZZ_REMEMBER_LOGIN", out var rememberToken))
    {
        try
        {
            var protector = context.RequestServices
                .GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("Zuzong.RememberLogin");

            var parts = protector.Unprotect(rememberToken).Split('|');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var userId) &&
                long.TryParse(parts[1], out var expiresTicks) &&
                new DateTimeOffset(expiresTicks, TimeSpan.Zero) > DateTimeOffset.UtcNow)
            {
                var db = context.RequestServices.GetRequiredService<QuanLyDienThoaiZuzongContext>();
                var user = await db.TaiKhoans.FirstOrDefaultAsync(x => x.MaTaiKhoan == userId && x.TrangThai == true);

                if (user != null)
                {
                    context.Session.SetString("MaTaiKhoan", user.MaTaiKhoan.ToString());
                    context.Session.SetString("TenDangNhap", user.TenDangNhap);
                    context.Session.SetString("HoTen", user.HoTen ?? "");
                    context.Session.SetString("VaiTro", user.VaiTro?.Trim() ?? "");
                    context.Session.SetString("Avatar", user.Avatar ?? "");
                }
            }
        }
        catch
        {
            context.Response.Cookies.Delete("ZZ_REMEMBER_LOGIN");
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
