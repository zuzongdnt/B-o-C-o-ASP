using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using dienthoai.Models;
using dienthoai.Services;
using System.IO;
namespace dienthoai.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;
        private readonly EmailService _emailService;
        private readonly IDataProtector _rememberProtector;

        // ✅ CHỈ MỘT constructor duy nhất
        public TaiKhoanController(
            QuanLyDienThoaiZuzongContext context,
            EmailService emailService,
            IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _emailService = emailService;
            _rememberProtector = dataProtectionProvider.CreateProtector("Zuzong.RememberLogin");
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private string GetSHA256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder();
            foreach (var b in bytes) builder.Append(b.ToString("X2"));
            return builder.ToString();
        }

        private bool IsStrongPassword(string password)
        {
            return Regex.IsMatch(password,
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\#^()\-_+=])[A-Za-z\d@$!%*?&\#^()\-_+=]{8,}$");
        }

        private bool IsValidVietnamPhone(string phone)
        {
            return Regex.IsMatch(phone?.Trim() ?? "", @"^(0|\+84)(3|5|7|8|9)\d{8}$");
        }

        private void SetLoginSession(TaiKhoan user)
        {
            HttpContext.Session.SetString("MaTaiKhoan", user.MaTaiKhoan.ToString());
            HttpContext.Session.SetString("TenDangNhap", user.TenDangNhap);
            HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
            HttpContext.Session.SetString("VaiTro", user.VaiTro?.Trim() ?? "");
            HttpContext.Session.SetString("Avatar", user.Avatar ?? "");
        }

        private void SaveRememberCookie(TaiKhoan user)
        {
            var expires = DateTimeOffset.UtcNow.AddDays(14);
            var token = _rememberProtector.Protect($"{user.MaTaiKhoan}|{expires.UtcTicks}");

            Response.Cookies.Append("ZZ_REMEMBER_LOGIN", token, new CookieOptions
            {
                Expires = expires,
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });
        }

        private string BuildVietnamAddress(string tinhThanh, string quanHuyen, string diaChiCuThe)
        {
            var parts = new[] { diaChiCuThe, quanHuyen, tinhThanh }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim());

            return string.Join(", ", parts);
        }

        private void ClearOtpSession()
        {
            foreach (var key in new[] { "OTP_Code", "OTP_Expiry", "OTP_TenDangNhap",
                                        "OTP_MatKhauHash", "OTP_HoTen", "OTP_Email" })
                HttpContext.Session.Remove(key);
        }

        // ==========================================
        // CRUD MẶC ĐỊNH (giữ nguyên)
        // ==========================================
        public IActionResult Index()
            => RedirectToAction(nameof(QuanLyNguoiDung));

        public async Task<IActionResult> QuanLyNguoiDung()
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("Login", "TaiKhoan");

            var taiKhoans = await _context.TaiKhoans
                .OrderByDescending(t => t.NgayTao)
                .ThenBy(t => t.TenDangNhap)
                .ToListAsync();

            ViewBag.TongTaiKhoan = taiKhoans.Count;
            ViewBag.TaiKhoanHoatDong = taiKhoans.Count(t => t.TrangThai == true);
            ViewBag.TaiKhoanBiKhoa = taiKhoans.Count(t => t.TrangThai != true);
            ViewBag.TaiKhoanAdmin = taiKhoans.Count(t => (t.VaiTro ?? "").Trim() == "Admin");
            ViewBag.HoTenAdmin = HttpContext.Session.GetString("HoTen");

            return View("Index", taiKhoans);
        }

        public async Task<IActionResult> Details(int? id, string? returnUrl = null)
        {
            if (id == null) return NotFound();
            var tk = await _context.TaiKhoans.FirstOrDefaultAsync(m => m.MaTaiKhoan == id);
            ViewBag.ReturnUrl = returnUrl;
            return tk == null ? NotFound() : View(tk);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        // Đã bổ sung Avatar vào Bind
        public async Task<IActionResult> Create([Bind("MaTaiKhoan,TenDangNhap,MatKhauHash,HoTen,Email,SoDienThoai,DiaChi,VaiTro,TrangThai,NgayTao,Avatar")] TaiKhoan taiKhoan)
        {
            if (!ModelState.IsValid) return View(taiKhoan);
            _context.Add(taiKhoan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tk = await _context.TaiKhoans.FindAsync(id);
            return tk == null ? NotFound() : View(tk);
        }

        [HttpPost, ValidateAntiForgeryToken]
        // Đã bổ sung Avatar vào Bind
        public async Task<IActionResult> Edit(int id, [Bind("MaTaiKhoan,TenDangNhap,HoTen,Email,SoDienThoai,DiaChi,VaiTro,TrangThai,NgayTao,Avatar")] TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTaiKhoan) return NotFound();
            ModelState.Remove(nameof(TaiKhoan.MatKhauHash));
            if (!ModelState.IsValid) return View(taiKhoan);

            var existing = await _context.TaiKhoans.FindAsync(id);
            if (existing == null) return NotFound();

            existing.TenDangNhap = taiKhoan.TenDangNhap;
            existing.HoTen = taiKhoan.HoTen;
            existing.Email = taiKhoan.Email;
            existing.SoDienThoai = taiKhoan.SoDienThoai;
            existing.DiaChi = taiKhoan.DiaChi;
            existing.VaiTro = taiKhoan.VaiTro;
            existing.TrangThai = taiKhoan.TrangThai;
            existing.NgayTao = taiKhoan.NgayTao;
            existing.Avatar = taiKhoan.Avatar;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaiKhoanExists(taiKhoan.MaTaiKhoan)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tk = await _context.TaiKhoans.FirstOrDefaultAsync(m => m.MaTaiKhoan == id);
            return tk == null ? NotFound() : View(tk);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null) _context.TaiKhoans.Remove(tk);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int id)
            => _context.TaiKhoans.Any(e => e.MaTaiKhoan == id);

        // ==========================================
        // 1. ĐĂNG NHẬP
        // ==========================================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string tenDangNhap, string matKhau, bool duyTriDangNhap = false)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            var user = _context.TaiKhoans.FirstOrDefault(u =>
                u.TenDangNhap == tenDangNhap &&
                u.MatKhauHash == GetSHA256Hash(matKhau) &&
                u.TrangThai == true);

            if (user != null)
            {
                SetLoginSession(user);
                if (duyTriDangNhap)
                    SaveRememberCookie(user);
                else
                    Response.Cookies.Delete("ZZ_REMEMBER_LOGIN");

                /*
                HttpContext.Session.SetString("MaTaiKhoan", user.MaTaiKhoan.ToString());
                HttpContext.Session.SetString("TenDangNhap", user.TenDangNhap);
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
                HttpContext.Session.SetString("VaiTro", user.VaiTro?.Trim() ?? "");
                
                // === ĐÃ THÊM LƯU AVATAR VÀO SESSION ===
                HttpContext.Session.SetString("Avatar", user.Avatar ?? "");
                */

                return user.VaiTro?.Trim() == "Admin"
                    ? RedirectToAction("Dashboard", "TaiKhoan")
                    : RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            return View();
        }

        // ==========================================
        // 2. DASHBOARD ADMIN
        // ==========================================
        public async Task<IActionResult> Dashboard()
{
    if (HttpContext.Session.GetString("VaiTro") != "Admin")
        return RedirectToAction("Login", "TaiKhoan");

    ViewBag.HoTenAdmin = HttpContext.Session.GetString("HoTen");

    var tongSoDon = await _context.DonHangs.CountAsync();
    var donHoanThanhQuery = _context.DonHangs
        .Where(d => d.TrangThaiDonHang == "Hoàn thành");

    var soDonHoanThanh = await donHoanThanhQuery.CountAsync();
    var doanhThu = await donHoanThanhQuery.SumAsync(d => (decimal?)d.TongTien) ?? 0;

    ViewBag.TongSoDon = tongSoDon;
    ViewBag.DoanhThu = doanhThu;
    ViewBag.GiaTriTB = soDonHoanThanh > 0 ? doanhThu / soDonHoanThanh : 0;

    ViewBag.TongSanPham = await _context.SanPhams.CountAsync();
    ViewBag.TongTaiKhoan = await _context.TaiKhoans.CountAsync();
    ViewBag.TaiKhoanMoi = await _context.TaiKhoans
        .OrderByDescending(t => t.NgayTao)
        .Take(5)
        .ToListAsync();
    ViewBag.TongTuCam = await _context.TuCamDanhGia.CountAsync();
    ViewBag.DonChoXuLy = await _context.DonHangs
        .CountAsync(d => d.TrangThaiDonHang == "Chờ xác nhận"
                      || d.TrangThaiDonHang == "Đang xử lý"
                      || d.TrangThaiDonHang == "Chờ xử lý");

    var trangThaiList = await _context.DonHangs
        .GroupBy(d => d.TrangThaiDonHang ?? "Chưa cập nhật")
        .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
        .OrderByDescending(x => x.SoLuong)
        .ToListAsync();

    ViewBag.LabelTrangThai = JsonSerializer.Serialize(trangThaiList.Select(x => x.TrangThai));
    ViewBag.DataTrangThai = JsonSerializer.Serialize(trangThaiList.Select(x => x.SoLuong));

    var top5SP = await _context.ChiTietDonHangs
        .Include(c => c.MaSpNavigation)
        .Where(c => c.MaSpNavigation != null)
        .GroupBy(c => c.MaSpNavigation!.TenSp)
        .Select(g => new { TenSP = g.Key, DaBan = g.Sum(c => c.SoLuong) })
        .OrderByDescending(x => x.DaBan)
        .Take(5)
        .ToListAsync();

    ViewBag.LabelTop5 = JsonSerializer.Serialize(top5SP.Select(x => x.TenSP));
    ViewBag.DataTop5 = JsonSerializer.Serialize(top5SP.Select(x => x.DaBan));

    var startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-5);
    var donHang6Thang = await _context.DonHangs
        .Where(d => d.NgayDatHang != null
                 && d.NgayDatHang >= startMonth
                 && d.TrangThaiDonHang == "Hoàn thành")
        .ToListAsync();

    var doanhThuTheoThang = Enumerable.Range(0, 6)
        .Select(i => startMonth.AddMonths(i))
        .Select(month => new
        {
            Thang = month.ToString("MM/yyyy"),
            Tien = donHang6Thang
                .Where(d => d.NgayDatHang!.Value.Year == month.Year
                         && d.NgayDatHang!.Value.Month == month.Month)
                .Sum(d => d.TongTien)
        })
        .ToList();

    ViewBag.LabelDoanhThu = JsonSerializer.Serialize(doanhThuTheoThang.Select(x => x.Thang));
    ViewBag.DataDoanhThu = JsonSerializer.Serialize(doanhThuTheoThang.Select(x => x.Tien));

    return View();
}

        // ==========================================
        // 3. ĐĂNG XUẤT
        // ==========================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("ZZ_REMEMBER_LOGIN");
            return RedirectToAction("Login", "TaiKhoan");
        }

        // ==========================================
        // 4. ĐĂNG KÝ - Bước 1: Validate + Gửi OTP
        // ==========================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string tenDangNhap, string matKhau,
            string xacNhanMatKhau, string hoTen, string email, string dongYDieuKhoan)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau) ||
                string.IsNullOrEmpty(xacNhanMatKhau) || string.IsNullOrEmpty(hoTen) ||
                string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ các thông tin bắt buộc!";
                return View();
            }

            if (!IsStrongPassword(matKhau))
            {
                ViewBag.Error = "Mật khẩu phải từ 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt!";
                return View();
            }

            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp!";
                return View();
            }

            if (dongYDieuKhoan != "on")
            {
                ViewBag.Error = "Bạn phải đồng ý với Điều khoản và Chính sách bảo mật!";
                return View();
            }

            if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == tenDangNhap))
            {
                ViewBag.Error = "Tên đăng nhập đã có người sử dụng!";
                return View();
            }

            if (await _context.TaiKhoans.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email này đã được đăng ký!";
                return View();
            }

            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP_Code", otp);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.Now.AddMinutes(5).ToString("o"));
            HttpContext.Session.SetString("OTP_TenDangNhap", tenDangNhap);
            HttpContext.Session.SetString("OTP_MatKhauHash", GetSHA256Hash(matKhau));
            HttpContext.Session.SetString("OTP_HoTen", hoTen);
            HttpContext.Session.SetString("OTP_Email", email);

            try
            {
                await _emailService.SendOtpAsync(email, hoTen, otp);
            }
            catch
            {
                ViewBag.Error = "Không thể gửi email. Kiểm tra lại địa chỉ email hoặc thử lại sau!";
                return View();
            }

            return RedirectToAction("VerifyOtp");
        }

        // ==========================================
        // 5. ĐĂNG KÝ - Bước 2: Xác thực OTP
        // ==========================================
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OTP_Code")))
                return RedirectToAction("Register");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string otpNhap)
        {
            var otpCode = HttpContext.Session.GetString("OTP_Code");
            var otpExpiry = HttpContext.Session.GetString("OTP_Expiry");

            if (string.IsNullOrEmpty(otpCode) || string.IsNullOrEmpty(otpExpiry))
            {
                TempData["Error"] = "Phiên xác thực hết hạn. Vui lòng đăng ký lại!";
                return RedirectToAction("Register");
            }

            if (DateTime.Parse(otpExpiry) < DateTime.Now)
            {
                ClearOtpSession();
                ViewBag.Error = "Mã OTP đã hết hạn (5 phút). Vui lòng đăng ký lại!";
                return View();
            }

            if (otpNhap?.Trim() != otpCode)
            {
                ViewBag.Error = "Mã OTP không chính xác. Vui lòng kiểm tra lại!";
                return View();
            }

            var newUser = new TaiKhoan
            {
                TenDangNhap = HttpContext.Session.GetString("OTP_TenDangNhap")!,
                MatKhauHash = HttpContext.Session.GetString("OTP_MatKhauHash")!,
                HoTen       = HttpContext.Session.GetString("OTP_HoTen"),
                Email       = HttpContext.Session.GetString("OTP_Email"),
                VaiTro      = "User",
                TrangThai   = true,
                NgayTao     = DateTime.Now,
                Avatar      = null // Tài khoản mới mặc định chưa có Avatar
            };

            _context.TaiKhoans.Add(newUser);
            await _context.SaveChangesAsync();
            ClearOtpSession();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp()
        {
            var email = HttpContext.Session.GetString("OTP_Email");
            var hoTen = HttpContext.Session.GetString("OTP_HoTen");

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");

            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP_Code", otp);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.Now.AddMinutes(5).ToString("o"));

            try
            {
                await _emailService.SendOtpAsync(email, hoTen!, otp);
                TempData["ResendSuccess"] = "Đã gửi lại mã OTP. Kiểm tra hộp thư!";
            }
            catch
            {
                TempData["ResendError"] = "Gửi lại thất bại. Thử lại sau!";
            }

            return RedirectToAction("VerifyOtp");
        }
        // ==========================================
        // CẬP NHẬT ẢNH ĐẠI DIỆN (AVATAR)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar(int MaTaiKhoan, IFormFile fileAvatar)
        {
            var user = await _context.TaiKhoans.FindAsync(MaTaiKhoan);
            if (user == null) return NotFound();

            if (fileAvatar != null && fileAvatar.Length > 0)
            {
                string tenHinhAnh = Guid.NewGuid().ToString() + "_" + fileAvatar.FileName;
                string thuMucLuu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                
                if (!Directory.Exists(thuMucLuu)) Directory.CreateDirectory(thuMucLuu);
                
                string duongDanLuu = Path.Combine(thuMucLuu, tenHinhAnh);
                using (var stream = new FileStream(duongDanLuu, FileMode.Create))
                {
                    await fileAvatar.CopyToAsync(stream);
                }

                user.Avatar = tenHinhAnh;
                await _context.SaveChangesAsync();
                
                // Cập nhật lại Session để thanh Menu thay đổi theo
                HttpContext.Session.SetString("Avatar", tenHinhAnh);
                TempData["SuccessMsg"] = "Đã cập nhật ảnh đại diện thành công!";
            }

            return RedirectToAction("Details", new { id = MaTaiKhoan });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int MaTaiKhoan, string hoTen, string email,
            string soDienThoai, string tinhThanh, string quanHuyen, string diaChiCuThe, string? returnUrl)
        {
            var currentUserId = HttpContext.Session.GetString("MaTaiKhoan");
            var currentRole = HttpContext.Session.GetString("VaiTro");
            if (currentUserId != MaTaiKhoan.ToString() && currentRole != "Admin")
                return Forbid();

            var user = await _context.TaiKhoans.FindAsync(MaTaiKhoan);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMsg"] = "Vui lòng nhập họ tên và email.";
                return RedirectToAction("Details", new { id = MaTaiKhoan, returnUrl });
            }

            if (string.IsNullOrWhiteSpace(soDienThoai) || !IsValidVietnamPhone(soDienThoai))
            {
                TempData["ErrorMsg"] = "Số điện thoại Việt Nam chưa đúng. Ví dụ: 0901234567.";
                return RedirectToAction("Details", new { id = MaTaiKhoan, returnUrl });
            }

            if (string.IsNullOrWhiteSpace(tinhThanh) || string.IsNullOrWhiteSpace(quanHuyen))
            {
                TempData["ErrorMsg"] = "Vui lòng chọn tỉnh/thành phố và nhập quận/huyện.";
                return RedirectToAction("Details", new { id = MaTaiKhoan, returnUrl });
            }

            bool emailUsed = await _context.TaiKhoans
                .AnyAsync(x => x.Email == email && x.MaTaiKhoan != MaTaiKhoan);
            if (emailUsed)
            {
                TempData["ErrorMsg"] = "Email này đã được tài khoản khác sử dụng.";
                return RedirectToAction("Details", new { id = MaTaiKhoan, returnUrl });
            }

            user.HoTen = hoTen.Trim();
            user.Email = email.Trim();
            user.SoDienThoai = soDienThoai.Trim();
            user.DiaChi = BuildVietnamAddress(tinhThanh, quanHuyen, diaChiCuThe);

            await _context.SaveChangesAsync();

            if (currentUserId == MaTaiKhoan.ToString())
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "");

            TempData["SuccessMsg"] = "Đã cập nhật thông tin tài khoản.";
            if (returnUrl == "checkout")
                return RedirectToAction("ThanhToan", "GioHang");

            return RedirectToAction("Details", new { id = MaTaiKhoan });
        }

        // ==========================================
        // ĐỔI MẬT KHẨU
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null) return NotFound();

            var currentUserId = HttpContext.Session.GetString("MaTaiKhoan");
            if (currentUserId != id.Value.ToString()) return Forbid();

            var user = await _context.TaiKhoans.FindAsync(id.Value);
            return user == null ? NotFound() : View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int MaTaiKhoan, string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            var currentUserId = HttpContext.Session.GetString("MaTaiKhoan");
            if (currentUserId != MaTaiKhoan.ToString()) return Forbid();

            var user = await _context.TaiKhoans.FindAsync(MaTaiKhoan);
            if (user == null) return NotFound();

            // Kiểm tra mật khẩu cũ có đúng không
            if (user.MatKhauHash != GetSHA256Hash(matKhauCu))
            {
                TempData["ErrorMsg"] = "Mật khẩu cũ không chính xác!";
                return RedirectToAction("ChangePassword", new { id = MaTaiKhoan });
            }

            // Kiểm tra mật khẩu mới và xác nhận
            if (matKhauMoi != xacNhanMatKhau)
            {
                TempData["ErrorMsg"] = "Mật khẩu nhập lại không khớp!";
                return RedirectToAction("ChangePassword", new { id = MaTaiKhoan });
            }

            if (!IsStrongPassword(matKhauMoi))
            {
                TempData["ErrorMsg"] = "Mật khẩu mới phải từ 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.";
                return RedirectToAction("ChangePassword", new { id = MaTaiKhoan });
            }

            // Đổi mật khẩu
            user.MatKhauHash = GetSHA256Hash(matKhauMoi);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Details", new { id = MaTaiKhoan });
        }
        // ==========================================
// QUÊN MẬT KHẨU - Bước 1: Nhập Email
// ==========================================
[HttpGet]
public IActionResult ForgotPassword() => View();

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(string email)
{
    if (string.IsNullOrEmpty(email))
    {
        ViewBag.Error = "Vui lòng nhập địa chỉ email!";
        return View();
    }

    // Kiểm tra xem email này có tài khoản nào không
    var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Email == email);
    if (user == null)
    {
        ViewBag.Error = "Địa chỉ email này chưa được đăng ký trong hệ thống!";
        return View();
    }

    // Tạo OTP
    string otp = new Random().Next(100000, 999999).ToString();
    
    // Lưu vào Session (dùng prefix Reset_ để phân biệt với đăng ký)
    HttpContext.Session.SetString("Reset_OTP", otp);
    HttpContext.Session.SetString("Reset_Expiry", DateTime.Now.AddMinutes(5).ToString("o"));
    HttpContext.Session.SetString("Reset_Email", email); // Lưu lại email để lát nữa đổi pass
    
    try
    {
        // Gửi mail (Dùng chung service với lúc đăng ký)
        await _emailService.SendOtpAsync(email, user.HoTen ?? "Thành viên", otp);
    }
    catch
    {
        ViewBag.Error = "Lỗi hệ thống khi gửi email. Vui lòng thử lại sau!";
        return View();
    }

    return RedirectToAction("VerifyForgotPasswordOtp");
}

// ==========================================
// QUÊN MẬT KHẨU - Bước 2: Xác thực OTP
// ==========================================
[HttpGet]
public IActionResult VerifyForgotPasswordOtp()
{
    if (string.IsNullOrEmpty(HttpContext.Session.GetString("Reset_OTP")))
        return RedirectToAction("ForgotPassword");
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult VerifyForgotPasswordOtp(string otpNhap)
{
    var otpCode = HttpContext.Session.GetString("Reset_OTP");
    var otpExpiry = HttpContext.Session.GetString("Reset_Expiry");

    if (string.IsNullOrEmpty(otpCode) || string.IsNullOrEmpty(otpExpiry))
    {
        TempData["Error"] = "Phiên làm việc hết hạn. Vui lòng yêu cầu lại!";
        return RedirectToAction("ForgotPassword");
    }

    if (DateTime.Parse(otpExpiry) < DateTime.Now)
    {
        ViewBag.Error = "Mã OTP đã hết hạn (5 phút). Vui lòng yêu cầu lại mã mới!";
        return View();
    }

    if (otpNhap?.Trim() != otpCode)
    {
        ViewBag.Error = "Mã OTP không chính xác!";
        return View();
    }

    // Đánh dấu là đã xác thực OTP thành công để cho phép vào trang đổi pass
    HttpContext.Session.SetString("Reset_Verified", "true");
    
    return RedirectToAction("ResetPassword");
}

// ==========================================
// QUÊN MẬT KHẨU - Bước 3: Đổi mật khẩu mới
// ==========================================
[HttpGet]
public IActionResult ResetPassword()
{
    // Chặn nếu người dùng tự gõ URL mà chưa qua bước xác thực OTP
    if (HttpContext.Session.GetString("Reset_Verified") != "true")
        return RedirectToAction("Login");
        
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(string matKhauMoi, string xacNhanMatKhau)
{
    // Kiểm tra chặn giống trên GET
    if (HttpContext.Session.GetString("Reset_Verified") != "true")
        return RedirectToAction("Login");

    if (!IsStrongPassword(matKhauMoi))
    {
        ViewBag.Error = "Mật khẩu phải từ 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt!";
        return View();
    }

    if (matKhauMoi != xacNhanMatKhau)
    {
        ViewBag.Error = "Mật khẩu nhập lại không khớp!";
        return View();
    }

    var email = HttpContext.Session.GetString("Reset_Email");
    var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Email == email);
    
    if (user != null)
    {
        // Cập nhật pass (Lưu ý: Dùng hàm Hash mà bạn đã viết)
        user.MatKhauHash = GetSHA256Hash(matKhauMoi);
        _context.Update(user);
        await _context.SaveChangesAsync();
    }

    // Xóa sạch session quên pass
    HttpContext.Session.Remove("Reset_OTP");
    HttpContext.Session.Remove("Reset_Expiry");
    HttpContext.Session.Remove("Reset_Email");
    HttpContext.Session.Remove("Reset_Verified");

    TempData["Success"] = "Lấy lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
    return RedirectToAction("Login");
}
    }
}
