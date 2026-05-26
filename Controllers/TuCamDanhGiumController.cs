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
    public class TuCamDanhGiumController : Controller
    {
        private readonly QuanLyDienThoaiZuzongContext _context;

        public TuCamDanhGiumController(QuanLyDienThoaiZuzongContext context)
        {
            _context = context;
        }

        // GET: TuCamDanhGium
        public async Task<IActionResult> Index()
        {
            return View(await _context.TuCamDanhGia.ToListAsync());
        }

        // GET: TuCamDanhGium/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuCamDanhGium = await _context.TuCamDanhGia
                .FirstOrDefaultAsync(m => m.MaTu == id);
            if (tuCamDanhGium == null)
            {
                return NotFound();
            }

            return View(tuCamDanhGium);
        }

        // GET: TuCamDanhGium/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TuCamDanhGium/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTu,TuKhoa,GhiChu")] TuCamDanhGium tuCamDanhGium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tuCamDanhGium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tuCamDanhGium);
        }

        // GET: TuCamDanhGium/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuCamDanhGium = await _context.TuCamDanhGia.FindAsync(id);
            if (tuCamDanhGium == null)
            {
                return NotFound();
            }
            return View(tuCamDanhGium);
        }

        // POST: TuCamDanhGium/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTu,TuKhoa,GhiChu")] TuCamDanhGium tuCamDanhGium)
        {
            if (id != tuCamDanhGium.MaTu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tuCamDanhGium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TuCamDanhGiumExists(tuCamDanhGium.MaTu))
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
            return View(tuCamDanhGium);
        }

        // GET: TuCamDanhGium/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuCamDanhGium = await _context.TuCamDanhGia
                .FirstOrDefaultAsync(m => m.MaTu == id);
            if (tuCamDanhGium == null)
            {
                return NotFound();
            }

            return View(tuCamDanhGium);
        }

        // POST: TuCamDanhGium/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tuCamDanhGium = await _context.TuCamDanhGia.FindAsync(id);
            if (tuCamDanhGium != null)
            {
                _context.TuCamDanhGia.Remove(tuCamDanhGium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TuCamDanhGiumExists(int id)
        {
            return _context.TuCamDanhGia.Any(e => e.MaTu == id);
        }
    }
}
