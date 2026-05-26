using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dienthoai.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
namespace dienthoai.Controllers
{
    public class DanhGiumController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;

        public DanhGiumController(QuanLyDienThoaiZuzongContext context)
        {
            _context = context;
        }

        // GET: DanhGium
        public async Task<IActionResult> Index()
        {
            var quanLyDienThoaiZuzongContext = _context.DanhGia.Include(d => d.MaSpNavigation).Include(d => d.MaTaiKhoanNavigation);
            return View(await quanLyDienThoaiZuzongContext.ToListAsync());
        }

        // GET: DanhGium/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGium = await _context.DanhGia
                .Include(d => d.MaSpNavigation)
                .Include(d => d.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);
            if (danhGium == null)
            {
                return NotFound();
            }

            return View(danhGium);
        }

        // GET: DanhGium/Create
        public IActionResult Create()
        {
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp");
            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan");
            return View();
        }

        // POST: DanhGium/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDanhGia,MaSp,MaTaiKhoan,SoSao,NoiDung,HinhAnhDg,TrangThaiHienThi,NgayDanhGia")] DanhGium danhGium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhGium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", danhGium.MaSp);
            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan", danhGium.MaTaiKhoan);
            return View(danhGium);
        }

        // GET: DanhGium/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGium = await _context.DanhGia.FindAsync(id);
            if (danhGium == null)
            {
                return NotFound();
            }
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", danhGium.MaSp);
            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan", danhGium.MaTaiKhoan);
            return View(danhGium);
        }

        // POST: DanhGium/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDanhGia,MaSp,MaTaiKhoan,SoSao,NoiDung,HinhAnhDg,TrangThaiHienThi,NgayDanhGia")] DanhGium danhGium)
        {
            if (id != danhGium.MaDanhGia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhGium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhGiumExists(danhGium.MaDanhGia))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", danhGium.MaSp);
            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoans, "MaTaiKhoan", "MaTaiKhoan", danhGium.MaTaiKhoan);
            return View(danhGium);
        }

        // GET: DanhGium/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGium = await _context.DanhGia
                .Include(d => d.MaSpNavigation)
                .Include(d => d.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);
            if (danhGium == null)
            {
                return NotFound();
            }

            return View(danhGium);
        }

        // POST: DanhGium/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhGium = await _context.DanhGia.FindAsync(id);
            if (danhGium != null)
            {
                _context.DanhGia.Remove(danhGium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanhGiumExists(int id)
        {
            return _context.DanhGia.Any(e => e.MaDanhGia == id);
        }
        // ==========================================
// THÊM ĐÁNH GIÁ (REVIEW) TỪ TRANG CHI TIẾT
// ==========================================
// Nhớ thêm using System.IO; ở đầu file nhé!

// Thay action PostReview trong DanhGiumController.cs bang doan nay
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> PostReview(int MaSp, int SoSao, string NoiDung, IFormFile fileHinhAnh)
{
    var maTkStr = HttpContext.Session.GetString("MaTaiKhoan");
    if (string.IsNullOrEmpty(maTkStr)) return RedirectToAction("Login", "TaiKhoan");

    int maTaiKhoan = int.Parse(maTkStr);

    bool daMuaVaHoanThanh = await _context.DonHangs
        .Include(d => d.ChiTietDonHangs)
        .AnyAsync(d => d.MaTaiKhoan == maTaiKhoan
                    && d.TrangThaiDonHang == "Hoàn thành"
                    && d.ChiTietDonHangs.Any(ct => ct.MaSp == MaSp));

    if (!daMuaVaHoanThanh)
    {
        TempData["ErrorReview"] = "Bạn chỉ có thể đánh giá sản phẩm sau khi đơn hàng đã hoàn thành.";
        return RedirectToAction("Details", "SanPham", new { id = MaSp });
    }

    bool daDanhGia = await _context.DanhGia
        .AnyAsync(d => d.MaTaiKhoan == maTaiKhoan && d.MaSp == MaSp);

    if (daDanhGia)
    {
        TempData["ErrorReview"] = "Bạn đã đánh giá sản phẩm này rồi.";
        return RedirectToAction("Details", "SanPham", new { id = MaSp });
    }

    var tuCamList = await _context.TuCamDanhGia.Select(t => t.TuKhoa.ToLower()).ToListAsync();
    if (!string.IsNullOrEmpty(NoiDung))
    {
        foreach (var tu in tuCamList)
        {
            if (NoiDung.ToLower().Contains(tu))
            {
                TempData["ErrorReview"] = "Đánh giá của bạn chứa từ ngữ vi phạm quy chuẩn cộng đồng!";
                return RedirectToAction("Details", "SanPham", new { id = MaSp });
            }
        }
    }

    string tenHinhAnh = null;
    if (fileHinhAnh != null && fileHinhAnh.Length > 0)
    {
        tenHinhAnh = Guid.NewGuid() + "_" + Path.GetFileName(fileHinhAnh.FileName);
        string thuMucLuu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        if (!Directory.Exists(thuMucLuu))
        {
            Directory.CreateDirectory(thuMucLuu);
        }

        string duongDanLuu = Path.Combine(thuMucLuu, tenHinhAnh);
        using (var stream = new FileStream(duongDanLuu, FileMode.Create))
        {
            await fileHinhAnh.CopyToAsync(stream);
        }
    }

    var review = new DanhGium
    {
        MaSp = MaSp,
        MaTaiKhoan = maTaiKhoan,
        SoSao = SoSao,
        NoiDung = NoiDung,
        HinhAnhDg = tenHinhAnh,
        NgayDanhGia = DateTime.Now,
        TrangThaiHienThi = true
    };

    _context.DanhGia.Add(review);
    await _context.SaveChangesAsync();

    TempData["SuccessReview"] = "Đã gửi đánh giá thành công!";
    return RedirectToAction("Details", "SanPham", new { id = MaSp });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> PostUserReply(int MaDanhGia, int MaSp, string NoiDung)
{
    var maTkStr = HttpContext.Session.GetString("MaTaiKhoan");
    if (!int.TryParse(maTkStr, out int maTaiKhoan))
        return RedirectToAction("Login", "TaiKhoan");

    var vaiTro = HttpContext.Session.GetString("VaiTro");
    bool laAdmin = vaiTro == "Admin";

    bool daMuaVaHoanThanh = await _context.DonHangs
        .Include(d => d.ChiTietDonHangs)
        .AnyAsync(d => d.MaTaiKhoan == maTaiKhoan
                    && d.TrangThaiDonHang == "Hoàn thành"
                    && d.ChiTietDonHangs.Any(ct => ct.MaSp == MaSp));

    if (!laAdmin && !daMuaVaHoanThanh)
    {
        TempData["ErrorReview"] = "Bạn chỉ có thể phản hồi sau khi đã mua hàng và đơn hàng hoàn thành.";
        return RedirectToAction("Details", "SanPham", new { id = MaSp });
    }

    if (string.IsNullOrWhiteSpace(NoiDung))
    {
        TempData["ErrorReview"] = "Vui lòng nhập nội dung phản hồi.";
        return RedirectToAction("Details", "SanPham", new { id = MaSp });
    }

    _context.DanhGiaPhanHois.Add(new DanhGiaPhanHoi
    {
        MaDanhGia = MaDanhGia,
        MaTaiKhoan = maTaiKhoan,
        NoiDung = NoiDung.Trim(),
        NgayPhanHoi = DateTime.Now,
        TrangThai = true
    });

    await _context.SaveChangesAsync();
    TempData["SuccessReview"] = "Đã gửi phản hồi.";
    return RedirectToAction("Details", "SanPham", new { id = MaSp });
}



// ==========================================
// NGƯỜI DÙNG TỰ XÓA ĐÁNH GIÁ CỦA MÌNH
// ==========================================
[HttpPost]
public async Task<IActionResult> DeleteReview(int MaDanhGia, int MaSp)
{
    var maTkStr = HttpContext.Session.GetString("MaTaiKhoan");
    var review = await _context.DanhGia.FindAsync(MaDanhGia);

    // Chỉ cho phép xóa nếu đúng là người đăng (hoặc là Admin)
    if (review != null && (review.MaTaiKhoan.ToString() == maTkStr || HttpContext.Session.GetString("VaiTro") == "Admin"))
    {
        _context.DanhGia.Remove(review);
        await _context.SaveChangesAsync();
    }
    return RedirectToAction("Details", "SanPham", new { id = MaSp });
}
// ==========================================
// ADMIN PHẢN HỒI ĐÁNH GIÁ CỦA KHÁCH
// ==========================================
[HttpPost]
public async Task<IActionResult> PostReply(int MaDanhGia, int MaSp, string PhanHoiAdmin)
{
    // Kiểm tra phải Admin mới được trả lời
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "Admin") return RedirectToAction("Login", "TaiKhoan");

    var review = await _context.DanhGia.FindAsync(MaDanhGia);
    if (review != null)
    {
        review.PhanHoi = PhanHoiAdmin;
        await _context.SaveChangesAsync();
        TempData["SuccessReview"] = "Đã gửi phản hồi thành công!";
    }
    
    return RedirectToAction("Details", "SanPham", new { id = MaSp });
}
    }
}
