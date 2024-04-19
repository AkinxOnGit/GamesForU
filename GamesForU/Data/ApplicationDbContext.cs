using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GamesForU.Models;

namespace GamesForU.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<GamesForU.Models.Publisher> Publisher { get; set; } = default!;
        public DbSet<GamesForU.Models.Games> Games { get; set; } = default!;
        public DbSet<GamesForU.Models.Category> Category { get; set; } = default!;
        public DbSet<GamesForU.Models.Pg> Pg { get; set; } = default!;
        public DbSet<GamesForU.Models.Orders> Orders { get; set; } = default!;
        public DbSet<GamesForU.Models.GamesOrders> GamesOrders { get; set; } = default!;
    }
}