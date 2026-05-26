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
    public class ChiTietGioHangController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;

        public ChiTietGioHangController(QuanLyDienThoaiZuzongContext context)
        {
            _context = context;
        }

        // GET: ChiTietGioHang
        public async Task<IActionResult> Index()
        {
            var quanLyDienThoaiZuzongContext = _context.ChiTietGioHangs.Include(c => c.MaGioHangNavigation).Include(c => c.MaSpNavigation);
            return View(await quanLyDienThoaiZuzongContext.ToListAsync());
        }

        // GET: ChiTietGioHang/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietGioHang = await _context.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Include(c => c.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaGioHang == id);
            if (chiTietGioHang == null)
            {
                return NotFound();
            }

            return View(chiTietGioHang);
        }

        // GET: ChiTietGioHang/Create
        public IActionResult Create()
        {
            ViewData["MaGioHang"] = new SelectList(_context.GioHangs, "MaGioHang", "MaGioHang");
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp");
            return View();
        }

        // POST: ChiTietGioHang/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaGioHang,MaSp,SoLuong")] ChiTietGioHang chiTietGioHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chiTietGioHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaGioHang"] = new SelectList(_context.GioHangs, "MaGioHang", "MaGioHang", chiTietGioHang.MaGioHang);
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", chiTietGioHang.MaSp);
            return View(chiTietGioHang);
        }

        // GET: ChiTietGioHang/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietGioHang = await _context.ChiTietGioHangs.FindAsync(id);
            if (chiTietGioHang == null)
            {
                return NotFound();
            }
            ViewData["MaGioHang"] = new SelectList(_context.GioHangs, "MaGioHang", "MaGioHang", chiTietGioHang.MaGioHang);
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", chiTietGioHang.MaSp);
            return View(chiTietGioHang);
        }

        // POST: ChiTietGioHang/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaGioHang,MaSp,SoLuong")] ChiTietGioHang chiTietGioHang)
        {
            if (id != chiTietGioHang.MaGioHang)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTietGioHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietGioHangExists(chiTietGioHang.MaGioHang))
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
            ViewData["MaGioHang"] = new SelectList(_context.GioHangs, "MaGioHang", "MaGioHang", chiTietGioHang.MaGioHang);
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", chiTietGioHang.MaSp);
            return View(chiTietGioHang);
        }

        // GET: ChiTietGioHang/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietGioHang = await _context.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Include(c => c.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaGioHang == id);
            if (chiTietGioHang == null)
            {
                return NotFound();
            }

            return View(chiTietGioHang);
        }

        // POST: ChiTietGioHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chiTietGioHang = await _context.ChiTietGioHangs.FindAsync(id);
            if (chiTietGioHang != null)
            {
                _context.ChiTietGioHangs.Remove(chiTietGioHang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChiTietGioHangExists(int id)
        {
            return _context.ChiTietGioHangs.Any(e => e.MaGioHang == id);
        }
    }
}
