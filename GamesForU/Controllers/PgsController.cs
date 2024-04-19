using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GamesForU.Data;
using GamesForU.Models;
using Microsoft.AspNetCore.Authorization;

namespace GamesForU.Controllers
{
    [Authorize(Roles = "Admin")]

    public class PgsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PgsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pgs
        public async Task<IActionResult> Index()
        {
              return _context.Pg != null ? 
                          View(await _context.Pg.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Pg'  is null.");
        }

        // GET: Pgs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pg == null)
            {
                return NotFound();
            }

            var pg = await _context.Pg
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pg == null)
            {
                return NotFound();
            }

            return View(pg);
        }

        // GET: Pgs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pgs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AgeRestriction")] Pg pg)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pg);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pg);
        }

        // GET: Pgs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pg == null)
            {
                return NotFound();
            }

            var pg = await _context.Pg.FindAsync(id);
            if (pg == null)
            {
                return NotFound();
            }
            return View(pg);
        }

        // POST: Pgs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AgeRestriction")] Pg pg)
        {
            if (id != pg.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pg);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PgExists(pg.Id))
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
            return View(pg);
        }

        // GET: Pgs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pg == null)
            {
                return NotFound();
            }

            var pg = await _context.Pg
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pg == null)
            {
                return NotFound();
            }

            return View(pg);
        }

        // POST: Pgs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pg == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Pg'  is null.");
            }
            var pg = await _context.Pg.FindAsync(id);
            if (pg != null)
            {
                _context.Pg.Remove(pg);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PgExists(int id)
        {
          return (_context.Pg?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
