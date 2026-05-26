using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using dienthoai.Models;
using dienthoai.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dienthoai.Controllers
{
    public class GioHangController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;
        private readonly IPaymentService _paymentService;

        private static readonly IReadOnlyList<DiscountRule> DiscountRules = new[]
        {
            new DiscountRule("ZUZONG10", "Giảm 10% tối đa 1.000.000đ cho đơn từ 5.000.000đ", 5_000_000m, 10m, null, 1_000_000m),
            new DiscountRule("ZUZONG500", "Giảm ngay 500.000đ cho đơn từ 12.000.000đ", 12_000_000m, null, 500_000m, null),
            new DiscountRule("SINHVIEN", "Giảm 5% tối đa 300.000đ cho đơn từ 3.000.000đ", 3_000_000m, 5m, null, 300_000m)
        };

        public GioHangController(QuanLyDienThoaiZuzongContext context, IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        private sealed record DiscountRule(
            string Code,
            string Description,
            decimal MinimumOrder,
            decimal? Percent,
            decimal? FixedAmount,
            decimal? MaxDiscount);

        private sealed record DiscountCalculation(
            string? Code,
            decimal Amount,
            string? Message,
            string? Error);

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("MaTaiKhoan"));

        private int CurrentUserId() => int.Parse(HttpContext.Session.GetString("MaTaiKhoan") ?? "0");

        private async Task<GioHang> GetOrCreateGioHangAsync(int maTaiKhoan)
        {
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            if (gioHang != null) return gioHang;

            gioHang = new GioHang { MaTaiKhoan = maTaiKhoan, NgayTao = DateTime.Now };
            _context.GioHangs.Add(gioHang);
            await _context.SaveChangesAsync();

            return gioHang;
        }

        private async Task<SanPhamBienThe?> GetSelectedVariantAsync(int maSp, int? maBienThe)
        {
            var query = _context.SanPhamBienThes
                .Where(x => x.MaSp == maSp && x.TrangThai);

            if (maBienThe.HasValue)
            {
                var selected = await query.FirstOrDefaultAsync(x => x.MaBienThe == maBienThe.Value);
                if (selected != null) return selected;
            }

            return await query
                .OrderByDescending(x => x.LaMacDinh)
                .ThenBy(x => x.DungLuong)
                .ThenBy(x => x.MauSac)
                .FirstOrDefaultAsync();
        }

        private async Task<ChiTietGioHang?> AddToCartAsync(int maSp, int? maBienThe)
        {
            var sanPham = await _context.SanPhams
                .FirstOrDefaultAsync(s => s.MaSp == maSp && s.TrangThai != false);
            if (sanPham == null) return null;

            int maTaiKhoan = CurrentUserId();
            var gioHang = await GetOrCreateGioHangAsync(maTaiKhoan);
            var bienThe = await GetSelectedVariantAsync(maSp, maBienThe);
            int? maBienTheChon = bienThe?.MaBienThe;

            var chiTietQuery = _context.ChiTietGioHangs
                .Where(c => c.MaGioHang == gioHang.MaGioHang && c.MaSp == maSp);

            chiTietQuery = maBienTheChon.HasValue
                ? chiTietQuery.Where(c => c.MaBienThe == maBienTheChon.Value)
                : chiTietQuery.Where(c => c.MaBienThe == null);

            var chiTiet = await chiTietQuery.FirstOrDefaultAsync();

            if (chiTiet != null)
            {
                chiTiet.SoLuong++;
            }
            else
            {
                chiTiet = new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaSp = maSp,
                    MaBienThe = maBienTheChon,
                    SoLuong = 1
                };
                _context.ChiTietGioHangs.Add(chiTiet);
            }

            await _context.SaveChangesAsync();
            return chiTiet;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "TaiKhoan");

            int maTaiKhoan = CurrentUserId();
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);

            if (gioHang == null) return View(Enumerable.Empty<ChiTietGioHang>());

            var chiTiet = await _context.ChiTietGioHangs
                .Include(c => c.MaSpNavigation)
                .Include(c => c.MaBienTheNavigation)
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .OrderByDescending(c => c.MaChiTietGioHang)
                .ToListAsync();

            return View(chiTiet);
        }

        public async Task<IActionResult> ThemVaoGio(int maSp, int? maBienThe)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "TaiKhoan");

            await AddToCartAsync(maSp, maBienThe);
            return RedirectToAction("Index", "SanPham");
        }

        public async Task<IActionResult> MuaNgay(int maSp, int? maBienThe)
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "Vui lòng đăng nhập để mua hàng!";
                return RedirectToAction("Login", "TaiKhoan");
            }

            await AddToCartAsync(maSp, maBienThe);
            return RedirectToAction("Index", "GioHang");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TangSoLuong(int maChiTietGioHang)
        {
            await ChangeQuantityAsync(maChiTietGioHang, 1);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiamSoLuong(int maChiTietGioHang)
        {
            await ChangeQuantityAsync(maChiTietGioHang, -1);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaSanPham(int maChiTietGioHang)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "TaiKhoan");

            int maTaiKhoan = CurrentUserId();
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            if (gioHang == null) return RedirectToAction(nameof(Index));

            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(c => c.MaChiTietGioHang == maChiTietGioHang && c.MaGioHang == gioHang.MaGioHang);
            if (chiTiet != null)
            {
                _context.ChiTietGioHangs.Remove(chiTiet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task ChangeQuantityAsync(int maChiTietGioHang, int delta)
        {
            if (!IsLoggedIn()) return;

            int maTaiKhoan = CurrentUserId();
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            if (gioHang == null) return;

            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(c => c.MaChiTietGioHang == maChiTietGioHang && c.MaGioHang == gioHang.MaGioHang);
            if (chiTiet == null) return;

            chiTiet.SoLuong += delta;
            if (chiTiet.SoLuong <= 0)
            {
                _context.ChiTietGioHangs.Remove(chiTiet);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<List<ChiTietGioHang>> GetCartItemsAsync(int maTaiKhoan)
        {
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            if (gioHang == null) return new List<ChiTietGioHang>();

            return await _context.ChiTietGioHangs
                .Include(c => c.MaSpNavigation)
                .Include(c => c.MaBienTheNavigation)
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .OrderByDescending(c => c.MaChiTietGioHang)
                .ToListAsync();
        }

        private ThanhToanViewModel BuildCheckoutModel(
            TaiKhoan user,
            List<ChiTietGioHang> items,
            string? phuongThucThanhToan = null,
            string? maGiamGia = null)
        {
            var tamTinh = items.Sum(c => c.SoLuong * (c.MaBienTheNavigation?.GiaBan ?? c.MaSpNavigation.GiaBan));
            var discount = CalculateDiscount(tamTinh, maGiamGia);

            ViewBag.TransferInfo = _paymentService.TransferInfo;

            return new ThanhToanViewModel
            {
                TaiKhoan = user,
                Items = items,
                PhuongThucThanhToan = NormalizePaymentMethod(phuongThucThanhToan),
                MaGiamGia = NormalizeDiscountCode(maGiamGia),
                MaGiamGiaDaApDung = discount.Error == null ? discount.Code : null,
                ThongBaoMaGiamGia = discount.Message,
                LoiMaGiamGia = discount.Error,
                TamTinh = tamTinh,
                GiamGia = discount.Error == null ? discount.Amount : 0,
                GoiYMaGiamGia = DiscountRules
                    .Select(x => new CheckoutDiscountOption { Code = x.Code, Description = x.Description })
                    .ToList()
            };
        }

        private static DiscountCalculation CalculateDiscount(decimal tamTinh, string? rawCode)
        {
            var code = NormalizeDiscountCode(rawCode);
            if (string.IsNullOrEmpty(code))
            {
                return new DiscountCalculation(null, 0, null, null);
            }

            var rule = DiscountRules.FirstOrDefault(x => x.Code == code);
            if (rule == null)
            {
                return new DiscountCalculation(code, 0, null, "Mã giảm giá không tồn tại.");
            }

            if (tamTinh < rule.MinimumOrder)
            {
                return new DiscountCalculation(
                    code,
                    0,
                    null,
                    $"Mã {code} áp dụng cho đơn từ {rule.MinimumOrder:N0}đ.");
            }

            var amount = rule.FixedAmount ?? tamTinh * (rule.Percent ?? 0) / 100m;
            if (rule.MaxDiscount.HasValue)
            {
                amount = Math.Min(amount, rule.MaxDiscount.Value);
            }

            amount = Math.Min(amount, tamTinh);
            return new DiscountCalculation(code, amount, $"Đã áp dụng mã {code}, giảm {amount:N0}đ.", null);
        }

        private static string NormalizeDiscountCode(string? code)
        {
            return (code ?? "").Trim().ToUpperInvariant();
        }

        private static string NormalizePaymentMethod(string? method)
        {
            return IsBankTransferMethod(method) ? "BANK" : "COD";
        }

        private static bool IsBankTransferMethod(string? method)
        {
            var value = (method ?? "").Trim();
            return value.Equals("BANK", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Chuyển khoản", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Chuyen khoan", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildPaymentDescription(string? method, string? discountCode)
        {
            var description = IsBankTransferMethod(method)
                ? "Chuyển khoản ngân hàng"
                : "Thanh toán khi nhận hàng";

            return string.IsNullOrWhiteSpace(discountCode)
                ? description
                : $"{description} - mã {discountCode}";
        }

        [HttpGet]
        public async Task<IActionResult> ThanhToan(string? maGiamGia = null)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "TaiKhoan");

            int maTaiKhoan = CurrentUserId();
            var user = await _context.TaiKhoans.FindAsync(maTaiKhoan);
            if (user == null) return RedirectToAction("Login", "TaiKhoan");

            if (string.IsNullOrWhiteSpace(user.SoDienThoai) || string.IsNullOrWhiteSpace(user.DiaChi))
            {
                TempData["ErrorMsg"] = "Vui lòng cập nhật số điện thoại và địa chỉ nhận hàng trước khi thanh toán.";
                return RedirectToAction("Details", "TaiKhoan", new { id = maTaiKhoan, returnUrl = "checkout" });
            }

            var chiTietGioHang = await GetCartItemsAsync(maTaiKhoan);
            if (chiTietGioHang.Count == 0) return RedirectToAction(nameof(Index));

            var model = BuildCheckoutModel(user, chiTietGioHang, "COD", maGiamGia);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(string PhuongThucThanhToan, string? MaGiamGia, string? submitAction)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "TaiKhoan");

            int maTaiKhoan = CurrentUserId();
            var user = await _context.TaiKhoans.FindAsync(maTaiKhoan);
            if (user == null) return RedirectToAction("Login", "TaiKhoan");

            if (string.IsNullOrWhiteSpace(user.SoDienThoai) || string.IsNullOrWhiteSpace(user.DiaChi))
            {
                TempData["ErrorMsg"] = "Vui lòng cập nhật số điện thoại và địa chỉ nhận hàng trước khi thanh toán.";
                return RedirectToAction("Details", "TaiKhoan", new { id = maTaiKhoan, returnUrl = "checkout" });
            }

            var chiTietGioHang = await GetCartItemsAsync(maTaiKhoan);

            if (chiTietGioHang.Count == 0) return RedirectToAction(nameof(Index));

            var model = BuildCheckoutModel(user, chiTietGioHang, PhuongThucThanhToan, MaGiamGia);
            if (string.Equals(submitAction, "apply", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(model.LoiMaGiamGia))
            {
                return View(model);
            }

            DonHang donHang = new DonHang
            {
                MaTaiKhoan = maTaiKhoan,
                TenNguoiNhan = user.HoTen ?? user.TenDangNhap,
                SoDienThoai = user.SoDienThoai ?? "",
                DiaChiGiaoHang = user.DiaChi ?? "",
                TongTien = model.TongThanhToan,
                PhuongThucThanhToan = BuildPaymentDescription(PhuongThucThanhToan, model.MaGiamGiaDaApDung),
                TrangThaiDonHang = "Chờ xác nhận",
                NgayDatHang = DateTime.Now
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in chiTietGioHang)
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSp = item.MaSp,
                    MaBienThe = item.MaBienThe,
                    SoLuong = item.SoLuong,
                    DonGia = item.MaBienTheNavigation?.GiaBan ?? item.MaSpNavigation.GiaBan
                });
            }

            _context.ChiTietGioHangs.RemoveRange(chiTietGioHang);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đặt hàng thành công!";
            TempData["SuccessMsg"] = IsBankTransferMethod(PhuongThucThanhToan)
                ? "Đặt hàng thành công. Bạn quét QR chuyển khoản để shop xác nhận nhanh hơn."
                : "Đặt hàng thành công!";
            return RedirectToAction("Details", "DonHang", new { id = donHang.MaDonHang });
        }

        [HttpPost]
        public async Task<IActionResult> ThemVaoGioAjax(int maSp, int? maBienThe)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var chiTiet = await AddToCartAsync(maSp, maBienThe);
            if (chiTiet == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            await _context.Entry(chiTiet).Reference(c => c.MaSpNavigation).LoadAsync();
            await _context.Entry(chiTiet).Reference(c => c.MaBienTheNavigation).LoadAsync();

            int maTaiKhoan = CurrentUserId();
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            int soLuong = gioHang == null
                ? 0
                : await _context.ChiTietGioHangs
                    .Where(c => c.MaGioHang == gioHang.MaGioHang)
                    .SumAsync(c => c.SoLuong);

            var tenSp = chiTiet.MaSpNavigation?.TenSp ?? "Sản phẩm";
            var bienTheText = chiTiet.MaBienTheNavigation == null
                ? ""
                : $"{chiTiet.MaBienTheNavigation.MauSac} - {chiTiet.MaBienTheNavigation.DungLuong}";

            return Json(new { success = true, tenSp, bienThe = bienTheText, soLuong });
        }
    }
}
