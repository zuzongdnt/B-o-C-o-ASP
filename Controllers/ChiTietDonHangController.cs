using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dienthoai.Models;

namespace dienthoai.Controllers
{
    public class ChiTietDonHangController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;

        public ChiTietDonHangController(QuanLyDienThoaiZuzongContext context)
        {
            _context = context;
        }

        // GET: ChiTietDonHang
        public async Task<IActionResult> Index()
        {
            var quanLyDienThoaiZuzongContext = _context.ChiTietDonHangs.Include(c => c.MaDonHangNavigation).Include(c => c.MaSpNavigation);
            return View(await quanLyDienThoaiZuzongContext.ToListAsync());
        }

        // GET: ChiTietDonHang/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietDonHang = await _context.ChiTietDonHangs
                .Include(c => c.MaDonHangNavigation)
                .Include(c => c.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);
            if (chiTietDonHang == null)
            {
                return NotFound();
            }

            return View(chiTietDonHang);
        }

        // GET: ChiTietDonHang/Create
        public IActionResult Create()
        {
            ViewData["MaDonHang"] = new SelectList(_context.DonHangs, "MaDonHang", "MaDonHang");
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp");
            return View();
        }

        // POST: ChiTietDonHang/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDonHang,MaSp,SoLuong,DonGia")] ChiTietDonHang chiTietDonHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chiTietDonHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDonHang"] = new SelectList(_context.DonHangs, "MaDonHang", "MaDonHang", chiTietDonHang.MaDonHang);
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", chiTietDonHang.MaSp);
            return View(chiTietDonHang);
        }

        // GET: ChiTietDonHang/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietDonHang = await _context.ChiTietDonHangs.FindAsync(id);
            if (chiTietDonHang == null)
            {
                return NotFound();
            }
            ViewData["MaDonHang"] = new SelectList(_context.DonHangs, "MaDonHang", "MaDonHang", chiTietDonHang.MaDonHang);
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", chiTietDonHang.MaSp);
            return View(chiTietDonHang);
        }

        // POST: ChiTietDonHang/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDonHang,MaSp,SoLuong,DonGia")] ChiTietDonHang chiTietDonHang)
        {
            if (id != chiTietDonHang.MaDonHang)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTietDonHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietDonHangExists(chiTietDonHang.MaDonHang))
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
            ViewData["MaDonHang"] = new SelectList(_context.DonHangs, "MaDonHang", "MaDonHang", chiTietDonHang.MaDonHang);
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", chiTietDonHang.MaSp);
            return View(chiTietDonHang);
        }

        // GET: ChiTietDonHang/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietDonHang = await _context.ChiTietDonHangs
                .Include(c => c.MaDonHangNavigation)
                .Include(c => c.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);
            if (chiTietDonHang == null)
            {
                return NotFound();
            }

            return View(chiTietDonHang);
        }

        // POST: ChiTietDonHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chiTietDonHang = await _context.ChiTietDonHangs.FindAsync(id);
            if (chiTietDonHang != null)
            {
                _context.ChiTietDonHangs.Remove(chiTietDonHang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChiTietDonHangExists(int id)
        {
            return _context.ChiTietDonHangs.Any(e => e.MaDonHang == id);
        }
    }
}
