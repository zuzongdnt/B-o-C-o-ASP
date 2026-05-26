using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using dienthoai.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace dienthoai.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ShopName = "Zuzong Store";
        private const string ShopOwner = "Dương Nhứt Thịnh";
        private const string ShopAddress = "Số 168, đường Nguyễn Văn Cừ (nối dài), phường An Bình, quận Ninh Kiều, Thành phố Cần Thơ";
        private const string ShopEmail = "zuzong68@gmail.com";
        private const string ShopFacebook = "https://www.facebook.com/duong.nhut.thinh";

        public SanPhamController(
            QuanLyDienThoaiZuzongContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int? maThuongHieu, string sortOrder, string keyword)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.MaThuongHieu = maThuongHieu;
            ViewBag.CurrentKeyword = keyword;

            var query = _context.SanPhams
                .Include(s => s.MaThuongHieuNavigation)
                .Include(s => s.SanPhamBienThes)
                .Include(s => s.DanhGia)
                .AsQueryable();

            if (maThuongHieu.HasValue)
            {
                query = query.Where(s => s.MaThuongHieu == maThuongHieu);
                ViewBag.TenHang = await _context.ThuongHieus
                    .Where(t => t.MaThuongHieu == maThuongHieu)
                    .Select(t => t.TenThuongHieu)
                    .FirstOrDefaultAsync();
            }
            else
            {
                ViewBag.TenHang = "Tất cả sản phẩm";
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s => s.TenSp.Contains(keyword));
                ViewBag.TenHang = $"Kết quả tìm kiếm cho: '{keyword}'";
            }

            switch (sortOrder)
            {
                case "gia_thap_cao":
                    query = query.OrderBy(s => s.GiaBan);
                    break;
                case "gia_cao_thap":
                    query = query.OrderByDescending(s => s.GiaBan);
                    break;
                case "khuyen_mai":
                    query = query.OrderByDescending(s => (s.GiaGoc ?? s.GiaBan) - s.GiaBan);
                    break;
                case "pho_bien":
                default:
                    query = query.OrderByDescending(s => s.NgayThem);
                    break;
            }

            return View(await query.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GoiYTimKiem(string? keyword, int? maThuongHieu)
        {
            var cleanKeyword = keyword?.Trim() ?? string.Empty;

            var query = _context.SanPhams
                .AsNoTracking()
                .Include(s => s.MaThuongHieuNavigation)
                .Include(s => s.SanPhamBienThes)
                .Where(s => s.TrangThai == true);

            if (maThuongHieu.HasValue)
            {
                query = query.Where(s => s.MaThuongHieu == maThuongHieu.Value);
            }

            if (!string.IsNullOrWhiteSpace(cleanKeyword))
            {
                query = query.Where(s =>
                    s.TenSp.Contains(cleanKeyword) ||
                    (s.MaThuongHieuNavigation != null &&
                     s.MaThuongHieuNavigation.TenThuongHieu.Contains(cleanKeyword)));

                query = query
                    .OrderByDescending(s => s.TenSp.StartsWith(cleanKeyword))
                    .ThenBy(s => s.TenSp);
            }
            else
            {
                query = query.OrderByDescending(s => s.NgayThem);
            }

            var products = await query
                .Take(8)
                .Select(s => new
                {
                    s.MaSp,
                    s.TenSp,
                    Brand = s.MaThuongHieuNavigation != null ? s.MaThuongHieuNavigation.TenThuongHieu : "",
                    s.GiaBan,
                    s.HinhAnh,
                    VariantPrice = s.SanPhamBienThes
                        .Where(v => v.TrangThai)
                        .OrderByDescending(v => v.LaMacDinh)
                        .ThenBy(v => v.GiaBan)
                        .Select(v => (decimal?)v.GiaBan)
                        .FirstOrDefault(),
                    VariantImage = s.SanPhamBienThes
                        .Where(v => v.TrangThai)
                        .OrderByDescending(v => v.LaMacDinh)
                        .ThenBy(v => v.GiaBan)
                        .Select(v => v.HinhAnh)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var suggestions = products.Select(p =>
            {
                var price = p.VariantPrice ?? p.GiaBan;
                var image = string.IsNullOrWhiteSpace(p.VariantImage) ? p.HinhAnh : p.VariantImage;

                return new
                {
                    id = p.MaSp,
                    name = p.TenSp,
                    brand = p.Brand,
                    price = FormatVnd(price),
                    image = string.IsNullOrWhiteSpace(image) ? "" : Url.Content("~/images/" + image),
                    url = Url.Action(nameof(Details), "SanPham", new { id = p.MaSp }) ?? $"/SanPham/Details/{p.MaSp}"
                };
            });

            return Json(new
            {
                success = true,
                items = suggestions
            });
        }
        [HttpGet]
        public async Task<IActionResult> TuVanChat(string? message)
        {
            var rawMessage = message?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawMessage))
            {
                return Json(new
                {
                    success = true,
                    reply = "Bạn cho mình biết ngân sách và nhu cầu nhé. Ví dụ: tầm 7 triệu chơi game, camera đẹp dưới 10 triệu, pin trâu cho sinh viên.",
                    products = Array.Empty<object>(),
                    source = "local"
                });
            }

            var normalized = NormalizeQuery(rawMessage);
            if (IsGeminiStatusQuestion(normalized))
            {
                var localReply = BuildGeminiStatusReply();
                return Json(new
                {
                    success = true,
                    reply = localReply,
                    products = Array.Empty<object>(),
                    source = "local"
                });
            }

            if (IsIdentityQuestion(normalized))
            {
                var localReply = BuildIdentityReply();
                return Json(new
                {
                    success = true,
                    reply = localReply,
                    products = Array.Empty<object>(),
                    source = "local"
                });
            }

            if (IsProductCountQuestion(normalized))
            {
                var activeProductCount = await _context.SanPhams
                    .AsNoTracking()
                    .CountAsync(s => s.TrangThai == true);
                var totalProductCount = await _context.SanPhams
                    .AsNoTracking()
                    .CountAsync();
                var brandCount = await _context.ThuongHieus
                    .AsNoTracking()
                    .CountAsync();

                var countReply = activeProductCount == totalProductCount
                    ? $"Trong ZuzongStore hiện có {activeProductCount} sản phẩm đang hiển thị, thuộc {brandCount} thương hiệu."
                    : $"Trong dữ liệu có {totalProductCount} sản phẩm, hiện web đang hiển thị {activeProductCount} sản phẩm còn bật trạng thái, thuộc {brandCount} thương hiệu.";

                return Json(new
                {
                    success = true,
                    reply = countReply,
                    products = Array.Empty<object>(),
                    source = "local"
                });
            }

            if (IsShopInfoQuestion(normalized))
            {
                var localReply = BuildShopInfoReply(normalized);
                var aiReply = await TryBuildGeminiReplyAsync(
                    rawMessage,
                    Array.Empty<ChatProductOption>(),
                    null,
                    Array.Empty<string>(),
                    null,
                    localReply,
                    isShopInfoQuestion: true);

                return Json(new
                {
                    success = true,
                    reply = aiReply ?? localReply,
                    products = Array.Empty<object>(),
                    source = aiReply == null ? "local" : "gemini"
                });
            }

            var budget = ExtractBudget(normalized);
            var needs = DetectNeeds(normalized);
            var brandPreference = DetectBrandPreference(normalized);
            var hasBuyingSignal = budget.HasValue ||
                needs.Count > 0 ||
                !string.IsNullOrWhiteSpace(brandPreference) ||
                IsBuyingQuestion(normalized);

            if (!hasBuyingSignal)
            {
                var localReply = "Mình chưa đủ ngữ cảnh để tư vấn máy cụ thể. Bạn hỏi mình theo kiểu: tầm bao nhiêu tiền, thích hãng nào, cần camera, pin hay chơi game; hoặc hỏi thông tin shop, mình sẽ trả lời đúng phần đó.";
                var aiReply = await TryBuildGeminiReplyAsync(
                    rawMessage,
                    Array.Empty<ChatProductOption>(),
                    null,
                    Array.Empty<string>(),
                    null,
                    localReply,
                    isShopInfoQuestion: false);

                return Json(new
                {
                    success = true,
                    reply = aiReply ?? localReply,
                    products = Array.Empty<object>(),
                    source = aiReply == null ? "local" : "gemini"
                });
            }

            var products = await _context.SanPhams
                .AsNoTracking()
                .Include(s => s.MaThuongHieuNavigation)
                .Include(s => s.SanPhamBienThes)
                .Where(s => s.TrangThai == true)
                .ToListAsync();

            var catalog = products
                .Select(ToChatProductOption)
                .Where(p => p.Price > 0)
                .ToList();

            var candidates = catalog
                .Select(p => new
                {
                    Product = p,
                    Score = ScoreProduct(p, budget, needs, brandPreference)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Product.Price)
                .Take(4)
                .Select(x => x.Product)
                .ToList();

            var localChatReply = BuildChatReply(candidates, budget, needs, brandPreference);
            var geminiReply = await TryBuildGeminiReplyAsync(
                rawMessage,
                candidates,
                budget,
                needs,
                brandPreference,
                localChatReply,
                isShopInfoQuestion: false);

            var reply = geminiReply ?? localChatReply;
            var resultProducts = candidates.Select(p => new
            {
                name = p.Name,
                brand = p.Brand,
                price = FormatVnd(p.Price),
                oldPrice = p.OldPrice.HasValue ? FormatVnd(p.OldPrice.Value) : null,
                image = string.IsNullOrWhiteSpace(p.Image) ? "" : Url.Content("~/images/" + p.Image),
                url = Url.Action(nameof(Details), "SanPham", new { id = p.Id, maBienThe = p.VariantId }),
                reason = BuildReason(p, budget, needs, brandPreference)
            });

            return Json(new
            {
                success = true,
                reply,
                products = resultProducts,
                source = geminiReply == null ? "local" : "gemini"
            });
        }
        public async Task<IActionResult> Details(int? id, int? maBienThe)
        {
            if (id == null) return NotFound();

       var sanPham = await _context.SanPhams
    .Include(s => s.MaThuongHieuNavigation)
    .Include(s => s.HinhAnhSanPhams)
    .Include(s => s.SanPhamBienThes)
    .Include(s => s.DanhGia)
        .ThenInclude(d => d.MaTaiKhoanNavigation)
    .Include(s => s.DanhGia)
        .ThenInclude(d => d.DanhGiaPhanHois)
            .ThenInclude(p => p.MaTaiKhoanNavigation)
    .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null) return NotFound();

            var bienThes = sanPham.SanPhamBienThes
                .Where(x => x.TrangThai)
                .OrderByDescending(x => x.LaMacDinh)
                .ThenBy(x => x.DungLuong)
                .ThenBy(x => x.MauSac)
                .ToList();

            var bienTheDangChon = maBienThe.HasValue
                ? bienThes.FirstOrDefault(x => x.MaBienThe == maBienThe.Value)
                : bienThes.FirstOrDefault(x => x.LaMacDinh) ?? bienThes.FirstOrDefault();

            bool coTheDanhGia = false;
            var maTkStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (int.TryParse(maTkStr, out int maTaiKhoan))
            {
                coTheDanhGia = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .AnyAsync(d => d.MaTaiKhoan == maTaiKhoan
                                && d.TrangThaiDonHang == "Hoàn thành"
                                && d.ChiTietDonHangs.Any(ct => ct.MaSp == id.Value));

                bool daDanhGia = await _context.DanhGia
                    .AnyAsync(d => d.MaTaiKhoan == maTaiKhoan && d.MaSp == id.Value);

                if (daDanhGia) coTheDanhGia = false;
            }

            ViewBag.BienThes = bienThes;
            ViewBag.BienTheDangChon = bienTheDangChon;
            ViewBag.CoTheDanhGia = coTheDanhGia;

            return View(sanPham);
        }

        public IActionResult Create()
        {
            ViewData["MaThuongHieu"] = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSp,MaThuongHieu,TenSp,GiaBan,HinhAnh,Chipset,Ram,Rom,Pin,Camera,TinhTrang,NguonGoc,SoLuongTon,TrangThai,NgayThem,GiaGoc")] SanPham sanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaThuongHieu"] = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", sanPham.MaThuongHieu);
            return View(sanPham);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams
                .Include(s => s.HinhAnhSanPhams)
                .Include(s => s.SanPhamBienThes)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null) return NotFound();

            ViewData["MaThuongHieu"] = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", sanPham.MaThuongHieu);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSp,MaThuongHieu,TenSp,GiaBan,HinhAnh,Chipset,Ram,Rom,Pin,Camera,TinhTrang,NguonGoc,SoLuongTon,TrangThai,NgayThem,GiaGoc")] SanPham sanPham)
        {
            if (id != sanPham.MaSp) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSp)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Edit), new { id = sanPham.MaSp });
            }

            ViewData["MaThuongHieu"] = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", sanPham.MaThuongHieu);
            return View(sanPham);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams
                .Include(s => s.MaThuongHieuNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            return sanPham == null ? NotFound() : View(sanPham);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null) _context.SanPhams.Remove(sanPham);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ThemAnhPhu(int maSp, List<IFormFile> fileHinhAnhPhu)
        {
            if (fileHinhAnhPhu != null && fileHinhAnhPhu.Count > 0)
            {
                string thuMucLuu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(thuMucLuu)) Directory.CreateDirectory(thuMucLuu);

                foreach (var file in fileHinhAnhPhu)
                {
                    string tenHinhAnh = Guid.NewGuid() + "_" + Path.GetFileName(file.FileName);
                    string duongDanLuu = Path.Combine(thuMucLuu, tenHinhAnh);

                    using (var stream = new FileStream(duongDanLuu, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                    {
                        MaSp = maSp,
                        DuongDanAnh = tenHinhAnh
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = maSp });
        }

        [HttpPost]
        public async Task<IActionResult> XoaAnhPhu(int maHinhAnh, int maSp)
        {
            var anh = await _context.HinhAnhSanPhams.FindAsync(maHinhAnh);
            if (anh != null)
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", anh.DuongDanAnh);
                if (System.IO.File.Exists(imgPath)) System.IO.File.Delete(imgPath);

                _context.HinhAnhSanPhams.Remove(anh);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = maSp });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBienThe(int maSp, string dungLuong, string mauSac, string maMau, decimal giaBan, decimal? giaGoc, string hinhAnh, int? soLuongTon, bool laMacDinh = false)
        {
            if (laMacDinh)
            {
                var olds = await _context.SanPhamBienThes.Where(x => x.MaSp == maSp).ToListAsync();
                foreach (var old in olds) old.LaMacDinh = false;
            }

            _context.SanPhamBienThes.Add(new SanPhamBienThe
            {
                MaSp = maSp,
                DungLuong = dungLuong,
                MauSac = mauSac,
                MaMau = maMau,
                GiaBan = giaBan,
                GiaGoc = giaGoc,
                HinhAnh = hinhAnh,
                SoLuongTon = soLuongTon,
                LaMacDinh = laMacDinh,
                TrangThai = true
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id = maSp });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaBienThe(int maSp, int maBienThe, string dungLuong, string mauSac, string maMau, decimal giaBan, decimal? giaGoc, string hinhAnh, int? soLuongTon, bool laMacDinh = false, bool trangThai = false)
        {
            var item = await _context.SanPhamBienThes.FindAsync(maBienThe);
            if (item == null) return NotFound();

            if (laMacDinh)
            {
                var olds = await _context.SanPhamBienThes
                    .Where(x => x.MaSp == maSp && x.MaBienThe != maBienThe)
                    .ToListAsync();
                foreach (var old in olds) old.LaMacDinh = false;
            }

            item.DungLuong = dungLuong;
            item.MauSac = mauSac;
            item.MaMau = maMau;
            item.GiaBan = giaBan;
            item.GiaGoc = giaGoc;
            item.HinhAnh = hinhAnh;
            item.SoLuongTon = soLuongTon;
            item.LaMacDinh = laMacDinh;
            item.TrangThai = trangThai;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id = maSp });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaBienThe(int maSp, int maBienThe)
        {
            var item = await _context.SanPhamBienThes.FindAsync(maBienThe);
            if (item != null)
            {
                _context.SanPhamBienThes.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = maSp });
        }

        private bool SanPhamExists(int id)
        {
            return _context.SanPhams.Any(e => e.MaSp == id);
        }

        private async Task<string?> TryBuildGeminiReplyAsync(
            string userMessage,
            IReadOnlyList<ChatProductOption> candidates,
            decimal? budget,
            IReadOnlyList<string> needs,
            string? brandPreference,
            string fallbackReply,
            bool isShopInfoQuestion)
        {
            var apiKey = _configuration["GeminiAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            var model = _configuration["GeminiAI:Model"];
            if (string.IsNullOrWhiteSpace(model))
            {
                model = "gemini-2.5-flash";
            }

            model = model.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
                ? model["models/".Length..]
                : model;

            var prompt = BuildGeminiPrompt(userMessage, candidates, budget, needs, brandPreference, fallbackReply, isShopInfoQuestion);
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.55,
                    maxOutputTokens = 800
                }
            };

            try
            {
                var client = _httpClientFactory.CreateClient("GeminiAI");
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent");

                request.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);
                request.Content = JsonContent.Create(requestBody);

                using var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);
                var aiText = ExtractGeminiText(document);

                return IsUsableGeminiReply(aiText, isShopInfoQuestion) ? aiText : null;
            }
            catch
            {
                return null;
            }
        }

        private static string BuildGeminiPrompt(
            string userMessage,
            IReadOnlyList<ChatProductOption> candidates,
            decimal? budget,
            IReadOnlyList<string> needs,
            string? brandPreference,
            string fallbackReply,
            bool isShopInfoQuestion)
        {
            var budgetText = budget.HasValue ? FormatVnd(budget.Value) : "khách chưa nói rõ";
            var needText = needs.Count > 0
                ? string.Join(", ", needs.Select(ToVietnameseNeed))
                : "khách chưa nói rõ";
            var brandText = string.IsNullOrWhiteSpace(brandPreference) ? "khách chưa nói rõ" : brandPreference;
            var productContext = candidates.Count == 0
                ? "Không có sản phẩm cụ thể được chọn. Ưu tiên trả lời thông tin shop hoặc hỏi thêm nhu cầu."
                : string.Join("\n", candidates.Select((p, index) =>
                {
                    var oldPrice = p.OldPrice.HasValue ? $"; giá gốc {FormatVnd(p.OldPrice.Value)}" : "";
                    var variant = string.IsNullOrWhiteSpace(p.VariantLabel) ? "" : $"; phiên bản {p.VariantLabel}";
                    var stock = p.Stock > 0 ? $"; tồn kho {p.Stock}" : "; tình trạng tồn kho chưa chắc chắn";
                    var link = p.VariantId.HasValue
                        ? $"/SanPham/Details/{p.Id}?maBienThe={p.VariantId.Value}"
                        : $"/SanPham/Details/{p.Id}";

                    return $"{index + 1}. {p.Name} | Hãng: {p.Brand} | Giá: {FormatVnd(p.Price)}{oldPrice}{variant}; RAM {p.Ram}; ROM {p.Rom}; Pin {p.Pin}; Camera {p.Camera}{stock}; Link {link}; Lý do hệ thống: {BuildReason(p, budget, needs.ToList(), brandPreference)}";
                }));

            return $@"Bạn là Gemini AI trả lời trong chatbox của {ShopName}, website bán điện thoại ZuzongStore.
Thông tin shop bắt buộc dùng đúng:
- Chủ shop: {ShopOwner}
- Địa chỉ: {ShopAddress}
- Facebook: {ShopFacebook}
- Email: {ShopEmail}
- Số điện thoại: chưa có trong dữ liệu, không được bịa.

Câu hỏi khách: {userMessage}
Đây là câu hỏi thông tin shop: {(isShopInfoQuestion ? "có" : "không")}
Ngân sách nhận diện: {budgetText}
Nhu cầu nhận diện: {needText}
Hãng khách quan tâm: {brandText}

Sản phẩm được hệ thống chọn từ database:
{productContext}

Câu trả lời fallback nếu cần: {fallbackReply}

Yêu cầu trả lời:
1. Trả lời bằng tiếng Việt tự nhiên, thân thiện, đúng giọng tư vấn viên ZuzongStore.
2. Dựa vào sản phẩm và thông tin shop ở trên; không tự bịa sản phẩm, giá, số điện thoại, bảo hành hay chính sách chưa có.
3. Nếu khách hỏi địa chỉ, liên hệ, chủ shop thì trả lời đúng thông tin shop.
4. Nếu tư vấn mua điện thoại, nêu 2-3 lựa chọn nổi bật, giá và lý do ngắn.
5. Tối đa 5 câu, không dùng bảng markdown.";
        }

        private static bool IsUsableGeminiReply(string? reply, bool isShopInfoQuestion)
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                return false;
            }

            var clean = reply.Trim();
            if (clean.Length < (isShopInfoQuestion ? 80 : 120))
            {
                return false;
            }

            var last = clean[^1];
            return last is '.' or '!' or '?' or '…';
        }
        private static string? ExtractGeminiText(JsonDocument document)
        {
            if (!document.RootElement.TryGetProperty("candidates", out var candidates) ||
                candidates.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var partsText = new List<string>();
            foreach (var candidate in candidates.EnumerateArray())
            {
                if (!candidate.TryGetProperty("content", out var content) ||
                    !content.TryGetProperty("parts", out var parts) ||
                    parts.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var textElement))
                    {
                        var value = textElement.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            partsText.Add(value.Trim());
                        }
                    }
                }
            }

            var text = string.Join("\n", partsText).Trim();
            return string.IsNullOrWhiteSpace(text) ? null : CleanGeminiReply(text);
        }

        private static string CleanGeminiReply(string reply)
        {
            return reply
                .Replace("**", "")
                .Replace("__", "")
                .Trim();
        }
        private static ChatProductOption ToChatProductOption(SanPham sanPham)
        {
            var activeVariants = sanPham.SanPhamBienThes
                .Where(v => v.TrangThai)
                .OrderBy(v => v.GiaBan)
                .ToList();
            var variant = activeVariants.FirstOrDefault(v => v.LaMacDinh) ?? activeVariants.FirstOrDefault();

            return new ChatProductOption
            {
                Id = sanPham.MaSp,
                VariantId = variant?.MaBienThe,
                Name = sanPham.TenSp,
                Brand = sanPham.MaThuongHieuNavigation?.TenThuongHieu ?? "",
                Price = variant?.GiaBan ?? sanPham.GiaBan,
                OldPrice = variant?.GiaGoc ?? sanPham.GiaGoc,
                Image = variant?.HinhAnh ?? sanPham.HinhAnh,
                VariantLabel = variant == null ? "" : $"{variant.DungLuong} - {variant.MauSac}",
                Chipset = sanPham.Chipset,
                Ram = sanPham.Ram,
                Rom = sanPham.Rom,
                Pin = sanPham.Pin,
                Camera = sanPham.Camera,
                Stock = variant?.SoLuongTon ?? sanPham.SoLuongTon ?? 0
            };
        }

        private static string NormalizeQuery(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant()
                .Replace('đ', 'd');
        }

        private static decimal? ExtractBudget(string normalizedMessage)
        {
            var cleaned = normalizedMessage.Replace(",", ".");
            var millionMatch = Regex.Match(cleaned, @"(\d+(?:\.\d+)?)\s*(trieu|tr|m)\b");
            if (millionMatch.Success &&
                decimal.TryParse(millionMatch.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var millionAmount))
            {
                return millionAmount * 1_000_000m;
            }

            var compactNumber = Regex.Replace(cleaned, @"[^\d]", "");
            if (compactNumber.Length >= 6 && decimal.TryParse(compactNumber, out var fullAmount))
            {
                return fullAmount;
            }

            var shortBudget = Regex.Match(cleaned, @"(?:duoi|tam|khoang|gia|ngan sach|budget)\s*(\d{1,2})(?:\b|$)");
            if (shortBudget.Success &&
                decimal.TryParse(shortBudget.Groups[1].Value, out var shortAmount))
            {
                return shortAmount * 1_000_000m;
            }

            return null;
        }

        private static List<string> DetectNeeds(string normalizedMessage)
        {
            var needs = new List<string>();

            if (ContainsAny(normalizedMessage, "game", "gaming", "genshin", "pubg", "lien quan", "toc chien", "fps", "chip"))
            {
                needs.Add("gaming");
            }

            if (ContainsAny(normalizedMessage, "camera", "chup", "anh", "video", "selfie", "quay", "song ao"))
            {
                needs.Add("camera");
            }

            if (ContainsAny(normalizedMessage, "pin", "trau", "sac", "lau", "ben"))
            {
                needs.Add("battery");
            }

            if (ContainsAny(normalizedMessage, "sinh vien", "hoc", "re", "tiet kiem", "co ban", "van phong"))
            {
                needs.Add("value");
            }

            return needs;
        }

        private static string? DetectBrandPreference(string normalizedMessage)
        {
            if (ContainsAny(normalizedMessage, "iphone", "apple", "ios")) return "iphone";
            if (ContainsAny(normalizedMessage, "samsung", "galaxy")) return "samsung";
            if (ContainsAny(normalizedMessage, "xiaomi", "redmi", "poco")) return "xiaomi";
            if (ContainsAny(normalizedMessage, "oppo")) return "oppo";
            if (ContainsAny(normalizedMessage, "vivo")) return "vivo";
            if (ContainsAny(normalizedMessage, "realme")) return "realme";
            return null;
        }

        private static bool ContainsAny(string haystack, params string[] needles)
        {
            return needles.Any(haystack.Contains);
        }

        private static bool IsGeminiStatusQuestion(string normalizedMessage)
        {
            return ContainsAny(
                normalizedMessage,
                "co xai api",
                "co dung api",
                "co that su xai api",
                "co that su dung api",
                "xai api cua tui",
                "dung api cua tui",
                "api gemini",
                "gemini co chay",
                "co xai gemini",
                "co dung gemini");
        }

        private string BuildGeminiStatusReply()
        {
            var hasGeminiKey = !string.IsNullOrWhiteSpace(_configuration["GeminiAI:ApiKey"]);
            return hasGeminiKey
                ? "Có, chatbox đang được cấu hình dùng Gemini API ở phía server. Nếu Gemini trả lỗi hoặc trả lời quá cụt thì mình tự chuyển sang câu trả lời dự phòng từ dữ liệu sản phẩm của ZuzongStore để web không bị đứng."
                : "Hiện server chưa đọc được Gemini API key, nên chatbox chỉ đang dùng câu trả lời dự phòng từ dữ liệu sản phẩm của ZuzongStore.";
        }

        private static bool IsIdentityQuestion(string normalizedMessage)
        {
            return ContainsAny(
                normalizedMessage,
                "tui la ai",
                "toi la ai",
                "minh la ai",
                "ban biet tui",
                "ban biet toi",
                "ban biet minh",
                "biet tui la ai",
                "biet toi la ai",
                "biet minh la ai",
                "dang noi chuyen voi ai");
        }

        private string BuildIdentityReply()
        {
            var displayName = HttpContext.Session.GetString("HoTen");
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = HttpContext.Session.GetString("TenDangNhap");
            }

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return $"Mình thấy bạn đang đăng nhập với tên {displayName}. Ngoài thông tin đăng nhập và câu bạn nhập trong chat, mình không tự biết thêm thông tin riêng tư nào khác.";
            }

            return "Mình chưa biết bạn là ai vì bạn chưa đăng nhập hoặc chưa cho mình tên trong đoạn chat này. Mình chỉ thấy câu bạn đang hỏi và dữ liệu sản phẩm của ZuzongStore thôi.";
        }

        private static bool IsProductCountQuestion(string normalizedMessage)
        {
            return ContainsAny(
                normalizedMessage,
                "bao nhieu san pham",
                "co bao nhieu san pham",
                "co may san pham",
                "so luong san pham",
                "tong san pham",
                "bao nhieu mau",
                "co may mau",
                "bao nhieu may",
                "trong day co bao nhieu");
        }

        private static bool IsBuyingQuestion(string normalizedMessage)
        {
            return ContainsAny(
                normalizedMessage,
                "mua",
                "nen mua",
                "tu van",
                "goi y",
                "chon",
                "may nao",
                "dien thoai",
                "phone",
                "smartphone",
                "gia",
                "trieu",
                "duoi",
                "tam",
                "khoang",
                "re",
                "tot",
                "man hinh",
                "hieu nang",
                "ram",
                "rom",
                "bo nho");
        }

        private static bool IsShopInfoQuestion(string normalizedMessage)
        {
            return ContainsAny(
                normalizedMessage,
                "dia chi",
                "o dau",
                "shop dau",
                "cua hang dau",
                "lien he",
                "contact",
                "hotline",
                "so dien thoai",
                "sdt",
                "facebook",
                "email",
                "chu shop",
                "chu cua hang",
                "thong tin shop",
                "thong tin cua hang");
        }

        private static string BuildShopInfoReply(string normalizedMessage)
        {
            var asksAddress = ContainsAny(normalizedMessage, "dia chi", "o dau", "shop dau", "cua hang dau");
            var asksContact = ContainsAny(normalizedMessage, "lien he", "contact", "hotline", "so dien thoai", "sdt", "facebook", "email");
            var asksOwner = ContainsAny(normalizedMessage, "chu shop", "chu cua hang", "thong tin shop", "thong tin cua hang");

            if (asksAddress && !asksContact && !asksOwner)
            {
                return $"{ShopName} ở {ShopAddress}.";
            }

            if (asksContact && !asksAddress && !asksOwner)
            {
                return $"Bạn có thể liên hệ {ShopName} qua Facebook {ShopFacebook} hoặc email {ShopEmail}. Hiện shop chưa có số điện thoại trong dữ liệu.";
            }

            if (asksOwner && !asksAddress && !asksContact)
            {
                return $"Chủ shop là {ShopOwner}. Bạn có thể liên hệ qua Facebook {ShopFacebook} hoặc email {ShopEmail}.";
            }

            return $"{ShopName} do {ShopOwner} phụ trách. Địa chỉ: {ShopAddress}. Liên hệ shop qua Facebook {ShopFacebook} hoặc email {ShopEmail}. Hiện dữ liệu chưa có số điện thoại.";
        }

        private static double ScoreProduct(ChatProductOption product, decimal? budget, List<string> needs, string? brandPreference)
        {
            var score = 10d;
            var text = NormalizeQuery($"{product.Name} {product.Brand} {product.Chipset} {product.Ram} {product.Rom} {product.Pin} {product.Camera}");

            if (budget.HasValue)
            {
                var ratio = product.Price / budget.Value;
                if (ratio <= 1m)
                {
                    score += 44 - Math.Abs((double)(1m - ratio)) * 12;
                }
                else if (ratio <= 1.15m)
                {
                    score += 18;
                }
                else
                {
                    score -= 28 + (double)(ratio - 1m) * 20;
                }
            }

            if (!string.IsNullOrWhiteSpace(brandPreference) && text.Contains(brandPreference))
            {
                score += 35;
            }

            if (needs.Contains("gaming"))
            {
                if (ContainsAny(text, "snapdragon", "dimensity", "a16", "a17", "a18", "ultra", "pro", "gaming")) score += 28;
                if (ContainsAny(text, "8gb", "12gb", "16gb")) score += 12;
            }

            if (needs.Contains("camera"))
            {
                if (ContainsAny(text, "pro", "ultra", "50mp", "108mp", "200mp", "ois", "iphone", "samsung")) score += 26;
            }

            if (needs.Contains("battery"))
            {
                if (ContainsAny(text, "5000", "5500", "6000", "pin trau", "sac nhanh")) score += 24;
            }

            if (needs.Contains("value"))
            {
                score += product.Price <= 8_000_000m ? 20 : 4;
            }

            score += product.Stock > 0 ? 6 : -8;
            return score;
        }

        private static string BuildChatReply(List<ChatProductOption> products, decimal? budget, List<string> needs, string? brandPreference)
        {
            if (products.Count == 0)
            {
                return "Mình chưa tìm thấy mẫu thật sự hợp với yêu cầu này. Bạn thử nói rõ hơn ngân sách, hãng thích dùng hoặc nhu cầu chính để mình lọc lại nhé.";
            }

            var budgetText = budget.HasValue ? $"ngân sách khoảng {FormatVnd(budget.Value)}" : "ngân sách bạn đang nhắm";
            var needText = needs.Count > 0
                ? string.Join(", ", needs.Select(ToVietnameseNeed))
                : "nhu cầu dùng hằng ngày";
            var brandText = string.IsNullOrWhiteSpace(brandPreference) ? "" : $" của {brandPreference.ToUpperInvariant()}";

            return $"Mình gợi ý {products.Count} mẫu{brandText} hợp với {budgetText}, ưu tiên {needText}. Bạn bấm vào từng máy để xem chi tiết cấu hình và biến thể giá.";
        }

        private static string BuildReason(ChatProductOption product, decimal? budget, List<string> needs, string? brandPreference)
        {
            var reasons = new List<string>();

            if (budget.HasValue && product.Price <= budget.Value)
            {
                reasons.Add("vừa ngân sách");
            }

            if (!string.IsNullOrWhiteSpace(brandPreference) &&
                NormalizeQuery($"{product.Brand} {product.Name}").Contains(brandPreference))
            {
                reasons.Add("đúng hãng bạn hỏi");
            }

            if (needs.Contains("gaming")) reasons.Add("hợp chơi game");
            if (needs.Contains("camera")) reasons.Add("ưu tiên camera");
            if (needs.Contains("battery")) reasons.Add("pin ổn cho dùng lâu");
            if (needs.Contains("value")) reasons.Add("giá dễ tiếp cận");
            if (!string.IsNullOrWhiteSpace(product.VariantLabel)) reasons.Add(product.VariantLabel);

            return reasons.Count == 0
                ? "Cấu hình cân bằng, dễ dùng hằng ngày."
                : string.Join(", ", reasons) + ".";
        }

        private static string ToVietnameseNeed(string need)
        {
            return need switch
            {
                "gaming" => "chơi game",
                "camera" => "camera đẹp",
                "battery" => "pin lâu",
                "value" => "giá tiết kiệm",
                _ => "dùng hằng ngày"
            };
        }

        private static string FormatVnd(decimal value)
        {
            return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", value);
        }

        private sealed class ChatProductOption
        {
            public int Id { get; set; }
            public int? VariantId { get; set; }
            public string Name { get; set; } = "";
            public string Brand { get; set; } = "";
            public decimal Price { get; set; }
            public decimal? OldPrice { get; set; }
            public string? Image { get; set; }
            public string VariantLabel { get; set; } = "";
            public string? Chipset { get; set; }
            public string? Ram { get; set; }
            public string? Rom { get; set; }
            public string? Pin { get; set; }
            public string? Camera { get; set; }
            public int Stock { get; set; }
        }
    }
}
