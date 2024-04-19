using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GamesForU.Data;
using GamesForU.Models;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Fluent;

namespace GamesForU.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public HistoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }
        private Task<IdentityUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }


        // GET: History
        public async Task<IActionResult> Index()
        {
            IdentityUser curruser = await GetCurrentUserAsync();
            if (curruser == null)
            {
                return NotFound();
            }
            bool isAdmin = await _userManager.IsInRoleAsync(curruser, "Admin");


            if (isAdmin)
            {
                var applicationDbContext = _context.GamesOrders.Include(g => g.Games).Include(g => g.Orders).Include(g => g.Orders.User).Where(o => (o.Orders.Invoice != null || o.Orders.Invoice != ""));
                var data = await applicationDbContext.ToListAsync();
                return View(data);
            }
            else
            {
                var orders = await _context.Orders
               .Where(o => o.IdentityUserId == curruser.Id && (o.Invoice != null || o.Invoice != "")).ToListAsync();

                if (orders == null)
                {
                    return View(NoContent());

                }

                List<GamesOrders>? list = new List<GamesOrders>();
                foreach(var order in orders)
                {
                    var gameOrder = await _context.GamesOrders.Include(g => g.Games).Include(g => g.Orders).Include(g => g.Orders.User)
                         .Where(go => go.OrdersId == order.Id).ToListAsync();

                    list.AddRange(gameOrder);
                }
                return View(list.OrderBy(o => o.OrdersId).ToList());

            }
        }

        [HttpGet, ActionName("ShowRechnung")]
        public async Task<IActionResult> ShowRechnung(int? id)
        {

            var order = await _context.Orders
             .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            var gameOrder = await _context.GamesOrders.Include(g => g.Games).Include(g => g.Orders).Include(g => g.Orders.User)
               .Where(go => go.OrdersId == order.Id).ToListAsync();

            if (gameOrder.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var model = InvoiceDocumentDataSource.GetInvoiceDetails(order, gameOrder);
            var document = new InvoiceDocument(model);
            document.GeneratePdfAndShow();

            return RedirectToAction(nameof(Index));

        }
        // GET: History/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.GamesOrders == null)
            {
                return NotFound();
            }

            var gamesOrders = await _context.GamesOrders
                .Include(g => g.Games)
                .Include(g => g.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gamesOrders == null)
            {
                return NotFound();
            }

            return View(gamesOrders);
        }

        // GET: History/Create
        public IActionResult Create()
        {
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Id");
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: History/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,GamesId,OrdersId")] GamesOrders gamesOrders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gamesOrders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Id", gamesOrders.GamesId);
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id", gamesOrders.OrdersId);
            return View(gamesOrders);
        }

        // GET: History/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GamesOrders == null)
            {
                return NotFound();
            }

            var gamesOrders = await _context.GamesOrders.FindAsync(id);
            if (gamesOrders == null)
            {
                return NotFound();
            }
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Id", gamesOrders.GamesId);
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id", gamesOrders.OrdersId);
            return View(gamesOrders);
        }

        // POST: History/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,GamesId,OrdersId")] GamesOrders gamesOrders)
        {
            if (id != gamesOrders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gamesOrders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GamesOrdersExists(gamesOrders.Id))
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
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Id", gamesOrders.GamesId);
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id", gamesOrders.OrdersId);
            return View(gamesOrders);
        }

        // GET: History/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.GamesOrders == null)
            {
                return NotFound();
            }

            var gamesOrders = await _context.GamesOrders
                .Include(g => g.Games)
                .Include(g => g.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gamesOrders == null)
            {
                return NotFound();
            }

            return View(gamesOrders);
        }

        // POST: History/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.GamesOrders == null)
            {
                return Problem("Entity set 'ApplicationDbContext.GamesOrders'  is null.");
            }
            var gamesOrders = await _context.GamesOrders.FindAsync(id);
            if (gamesOrders != null)
            {
                _context.GamesOrders.Remove(gamesOrders);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GamesOrdersExists(int id)
        {
          return (_context.GamesOrders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
