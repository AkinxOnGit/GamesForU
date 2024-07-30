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
using Microsoft.AspNetCore.Identity;

namespace GamesForU.Controllers
{

    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GamesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }


        // GET: Games/Edit/5
        public async Task<IActionResult> AddToCard(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            IdentityUser user = await GetCurrentUserAsync();

            if (user == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Pg)
                .Include(g => g.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.IdentityUserId == user.Id && (o.Invoice == null || o.Invoice == ""));

            if (order == null)
            {
                order = new Orders
                {
                    IdentityUserId = user.Id,
                    Date = DateTime.Now,
                    Invoice = ""
                };

                _context.Add(order);
                await _context.SaveChangesAsync();
            }

            var gameOrder = await _context.GamesOrders
                .FirstOrDefaultAsync(go => go.GamesId == game.Id && go.OrdersId == order.Id);

            if (gameOrder == null)
            {
                gameOrder = new GamesOrders (1,game.Id,order.Id);
                _context.Add(gameOrder);
                await _context.SaveChangesAsync();
            }
            else
            {
                gameOrder.Amount++;
                _context.Update(gameOrder);
                await _context.SaveChangesAsync();
            }


            TempData["SuccessMessage"] = "Game " + game.Name + " was added to the shopping cart";
            return RedirectToAction(nameof(Index));
        }

        private Task<IdentityUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        // GET: Games
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParam"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["PublisherSortParam"] = sortOrder == "Publisher" ? "publisher_desc" : "Publisher";
            ViewData["CurrentFilter"] = searchString;

            var games = from g in _context.Games.Include(g => g.Pg).Include(g => g.Publisher)
                        select g;

            if (!String.IsNullOrEmpty(searchString))
            {
                games = games.Where(g => g.Name.Contains(searchString) || g.Publisher.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    games = games.OrderByDescending(g => g.Name);
                    break;
                case "Price":
                    games = games.OrderBy(g => g.Price);
                    break;
                case "price_desc":
                    games = games.OrderByDescending(g => g.Price);
                    break;
                case "Publisher":
                    games = games.OrderBy(g => g.Publisher.Name);
                    break;
                case "publisher_desc":
                    games = games.OrderByDescending(g => g.Publisher.Name);
                    break;
                default:
                    games = games.OrderBy(g => g.Name);
                    break;
            }

            return View(await games.ToListAsync());
        }


        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Games == null)
            {
                return NotFound();
            }

            var games = await _context.Games
                .Include(g => g.Pg)
                .Include(g => g.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (games == null)
            {
                return NotFound();
            }

            return View(games);
        }
        [Authorize(Roles = "Admin")]

        // GET: Games/Create
        public IActionResult Create()
        {
            ViewData["PgId"] = new SelectList(_context.Pg, "Id", "AgeRestriction");
            ViewData["PublisherId"] = new SelectList(_context.Publisher, "Id", "Name");
            return View();
        }

        // POST: Games/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Price,Amount,Name,PublisherId,PgId")] Games games)
        {
            if (ModelState.IsValid)
            {
                _context.Add(games);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PgId"] = new SelectList(_context.Pg, "Id", "AgeRestriction", games.PgId);
            ViewData["PublisherId"] = new SelectList(_context.Publisher, "Id", "Name", games.PublisherId);
            return View(games);
        }
        [Authorize(Roles = "Admin")]

        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Games == null)
            {
                return NotFound();
            }

            var games = await _context.Games.FindAsync(id);
            if (games == null)
            {
                return NotFound();
            }
            ViewData["PgId"] = new SelectList(_context.Pg, "Id", "AgeRestriction", games.PgId);
            ViewData["PublisherId"] = new SelectList(_context.Publisher, "Id", "Name", games.PublisherId);
            return View(games);
        }

        // POST: Games/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Price,Amount,Name,PublisherId,PgId")] Games games)
        {
            if (id != games.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(games);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GamesExists(games.Id))
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
            ViewData["PgId"] = new SelectList(_context.Pg, "Id", "AgeRestriction", games.PgId);
            ViewData["PublisherId"] = new SelectList(_context.Publisher, "Id", "Name", games.PublisherId);
            return View(games);
        }
        [Authorize(Roles = "Admin")]

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Games == null)
            {
                return NotFound();
            }

            var games = await _context.Games
                .Include(g => g.Pg)
                .Include(g => g.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (games == null)
            {
                return NotFound();
            }

            return View(games);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Games == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Games'  is null.");
            }
            var games = await _context.Games.FindAsync(id);
            if (games != null)
            {
                _context.Games.Remove(games);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GamesExists(int id)
        {
          return (_context.Games?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
