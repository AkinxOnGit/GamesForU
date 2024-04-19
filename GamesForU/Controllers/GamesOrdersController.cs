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
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using IContainer = QuestPDF.Infrastructure.IContainer;
using System.Diagnostics;


namespace GamesForU.Controllers
{
    public class GamesOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public GamesOrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }
        private Task<IdentityUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }


        // GET: GamesOrders
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
                var applicationDbContext = _context.GamesOrders.Include(g => g.Games).Include(g => g.Orders).Include(g => g.Orders.User).Where(o=>(o.Orders.Invoice == null || o.Orders.Invoice == ""));
                var data = await applicationDbContext.ToListAsync();
                return View(data);
            }
            else
            {
                var order = await _context.Orders
               .FirstOrDefaultAsync(o => o.IdentityUserId == curruser.Id && (o.Invoice == null || o.Invoice == ""));

                if (order == null)
                {
                    order = new Orders
                    {
                        IdentityUserId = curruser.Id,
                        Date = DateTime.Now,
                        Invoice = ""
                    };

                    _context.Add(order);
                    await _context.SaveChangesAsync();
                }

                var gameOrder = await _context.GamesOrders.Include(g => g.Games).Include(g => g.Orders).Include(g => g.Orders.User)
                     .Where(go => go.OrdersId == order.Id).ToListAsync();

                return View(gameOrder);

            }
        }


        // GET: GamesOrders
        public async Task<IActionResult> RemoveFromCard(int? id)
        {
            IdentityUser curruser = await GetCurrentUserAsync();
            if (curruser == null)
            {
                return NotFound();
            }

            var currGameOrder = await _context.GamesOrders
           .FirstOrDefaultAsync(go => go.Id == id);



            if (currGameOrder.Amount == 1)
            {
                _context.GamesOrders.Remove(currGameOrder);
                await _context.SaveChangesAsync();
            }
            else
            {
                currGameOrder.Amount--;
                _context.Update(currGameOrder);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        [Authorize(Roles = "Admin")]

        // GET: GamesOrders/Details/5
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
        [Authorize(Roles = "Admin")]

        // GET: GamesOrders/Create
        public IActionResult Create()
        {
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Name");
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: GamesOrders/Create
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
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Name", gamesOrders.GamesId);
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id", gamesOrders.OrdersId);
            return View(gamesOrders);
        }
        [Authorize(Roles = "Admin")]

        // GET: GamesOrders/Edit/5
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
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Name", gamesOrders.GamesId);
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id", gamesOrders.OrdersId);
            return View(gamesOrders);
        }
        [Authorize(Roles = "Admin")]

        // POST: GamesOrders/Edit/5
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
            ViewData["GamesId"] = new SelectList(_context.Games, "Id", "Name", gamesOrders.GamesId);
            ViewData["OrdersId"] = new SelectList(_context.Orders, "Id", "Id", gamesOrders.OrdersId);
            return View(gamesOrders);
        }
        [Authorize(Roles = "Admin")]


        // GET: GamesOrders/Delete/5
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
        [Authorize(Roles = "Admin")]

        // POST: GamesOrders/Delete/5
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

        //PDF GENERATION
        [HttpPost, ActionName("Buy")]
        public async Task<IActionResult> Buy()
        {

            IdentityUser curruser = await GetCurrentUserAsync();
            if (curruser == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
             .FirstOrDefaultAsync(o => o.IdentityUserId == curruser.Id && (o.Invoice == null || o.Invoice == ""));
            if (order == null)
            {
                return NotFound();
            }
            var gameOrder = await _context.GamesOrders.Include(g => g.Games).Include(g => g.Orders).Include(g => g.Orders.User)
               .Where(go => go.OrdersId == order.Id).ToListAsync();
           
            if(gameOrder.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var model = InvoiceDocumentDataSource.GetInvoiceDetails(order,gameOrder);
            var document = new InvoiceDocument(model);
            document.GeneratePdfAndShow();


            order.Invoice = "RECHNUNG";
            _context.Update(order);
            await _context.SaveChangesAsync();

       
            return RedirectToAction(nameof(Index));

        }
    }
    public class InvoiceModel
    {
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        public List<OrderItem> Items { get; set; }
        public string Comments { get; set; }
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public static class InvoiceDocumentDataSource
    {
        private static Random Random = new Random();

        public static InvoiceModel GetInvoiceDetails(Orders order, List<GamesOrders> gameOrder)
        {
            var orderItems = new List<OrderItem>();

            foreach (var item in order.GamesOrders)
            {
                orderItems.Add(new OrderItem
                {
                    Name = item.Games.Name,
                    Price = Int16.Parse(item.Games.Price)*item.Amount,
                    Quantity = item.Amount
                });
            }
            return new InvoiceModel
            {
                InvoiceNumber = Random.Next(1_000, 10_000),
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now + TimeSpan.FromDays(14),


                Items = orderItems,
                Comments = Placeholders.Paragraph()
            };
        }

        private static OrderItem GenerateRandomOrderItem()
        {
            return new OrderItem
            {
                Name = Placeholders.Label(),
                Price = (decimal)Math.Round(Random.NextDouble() * 100, 2),
                Quantity = Random.Next(1, 10)
            };
        }
    }


public class InvoiceDocument : IDocument
    {
        public InvoiceModel Model { get; }

        public InvoiceDocument(InvoiceModel model)
        {
            Model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);


                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Invoice #{Model.InvoiceNumber}").Style(titleStyle);

                    column.Item().Text(text =>
                    {
                        text.Span("Issue date: ").SemiBold();
                        text.Span($"{Model.IssueDate:d}");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Due date: ").SemiBold();
                        text.Span($"{Model.DueDate:d}");
                    });
                });

                row.ConstantItem(100).Height(50).Placeholder();
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);

                column.Item().Element(ComposeTable);


                var totalPrice = Model.Items.Sum(x => x.Price * x.Quantity);
                column.Item().AlignRight().Text($"Grand total: {totalPrice}$").FontSize(14);


                if (!string.IsNullOrWhiteSpace(Model.Comments))
                    column.Item().PaddingTop(25).Element(ComposeComments);
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Product");
                    header.Cell().Element(CellStyle).AlignRight().Text("Unit price");
                    header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                // step 3
                foreach (var item in Model.Items)
                {
                    table.Cell().Element(CellStyle).Text(Model.Items.IndexOf(item) + 1);
                    table.Cell().Element(CellStyle).Text(item.Name);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price}$");
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price * item.Quantity}$");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
        }

        void ComposeComments(IContainer container)
        {
            container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text("Comments").FontSize(14);
                column.Item().Text(Model.Comments);
            });
        }
    }
}
