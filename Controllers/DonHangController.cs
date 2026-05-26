using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dienthoai.Models;
using dienthoai.Services;
using Microsoft.AspNetCore.Http;

namespace dienthoai.Controllers
{
    public class DonHangController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;
        private readonly IPaymentService _paymentService;

        public DonHangController(QuanLyDienThoaiZuzongContext context, IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        private bool CanCancelOrder(string? status)
        {
            var normalized = (status ?? "").Trim();
            return normalized == "Chờ xác nhận"
                || normalized == "Chờ xử lý"
                || normalized == "Đang xử lý";
        }

        private static bool IsBankTransferMethod(string? method)
        {
            var value = (method ?? "").Trim();
            return value.Contains("Chuyển khoản", System.StringComparison.OrdinalIgnoreCase)
                || value.Contains("Chuyen khoan", System.StringComparison.OrdinalIgnoreCase)
                || value.Equals("BANK", System.StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            var donHangs = _context.DonHangs
                .Include(d => d.MaTaiKhoanNavigation)
                .OrderByDescending(d => d.NgayDatHang);
            return View(await donHangs.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs
                .Include(d => d.MaTaiKhoanNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaBienTheNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);

            if (donHang == null) return NotFound();

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTkStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro != "Admin" && (!int.TryParse(maTkStr, out int maTk) || donHang.MaTaiKhoan != maTk))
                return Forbid();

            ViewBag.CoTheHuy = vaiTro != "Admin" && CanCancelOrder(donHang.TrangThaiDonHang);
            var tamTinh = donHang.ChiTietDonHangs?.Sum(x => x.DonGia * x.SoLuong) ?? donHang.TongTien;
            ViewBag.TamTinh = tamTinh;
            ViewBag.GiamGia = Math.Max(0, tamTinh - donHang.TongTien);

            if (IsBankTransferMethod(donHang.PhuongThucThanhToan))
            {
                ViewBag.TransferInfo = _paymentService.TransferInfo;
                ViewBag.SePayQrUrl = _paymentService.GenerateSePayQrUrl(donHang.TongTien, donHang.MaDonHang);
            }

            return View(donHang);
        }

        public IActionResult Create()
        {
            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDonHang,MaTaiKhoan,TenNguoiNhan,SoDienThoai,DiaChiGiaoHang,TongTien,PhuongThucThanhToan,TrangThaiDonHang,LyDoHuy,NgayDatHang")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan", donHang.MaTaiKhoan);
            return View(donHang);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan", donHang.MaTaiKhoan);
            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDonHang,MaTaiKhoan,TenNguoiNhan,SoDienThoai,DiaChiGiaoHang,TongTien,PhuongThucThanhToan,TrangThaiDonHang,LyDoHuy,NgayDatHang")] DonHang donHang)
        {
            if (id != donHang.MaDonHang) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(donHang.MaDonHang)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan", donHang.MaTaiKhoan);
            return View(donHang);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs
                .Include(d => d.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);
            return donHang == null ? NotFound() : View(donHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null) _context.DonHangs.Remove(donHang);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LichSuDonHang()
        {
            var maTkStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrEmpty(maTkStr))
                return RedirectToAction("Login", "TaiKhoan");

            int maTk = int.Parse(maTkStr);

            var danhSachDonHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaBienTheNavigation)
                .Where(d => d.MaTaiKhoan == maTk)
                .OrderByDescending(d => d.NgayDatHang)
                .ToListAsync();

            return View(danhSachDonHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThaiMoi)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                donHang.TrangThaiDonHang = trangThaiMoi;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id, string lyDoHuy)
        {
            var maTkStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrEmpty(maTkStr))
                return RedirectToAction("Login", "TaiKhoan");

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            if (!int.TryParse(maTkStr, out int maTk) || donHang.MaTaiKhoan != maTk)
                return Forbid();

            if (!CanCancelOrder(donHang.TrangThaiDonHang))
            {
                TempData["ErrorMsg"] = "Đơn hàng này không còn ở trạng thái có thể hủy.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                TempData["ErrorMsg"] = "Vui lòng nhập lý do hủy đơn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            donHang.TrangThaiDonHang = "Đã hủy";
            donHang.LyDoHuy = lyDoHuy.Trim();
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Đã hủy đơn hàng.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.MaDonHang == id);
        }
    }
}
