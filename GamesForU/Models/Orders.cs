using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GamesForU.Models
{
    public class Orders
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string? Invoice { get; set; }


        [ForeignKey("IdentityUser")]
        public string IdentityUserId { get; set; }
        public virtual IdentityUser? User { get; set; }

        public virtual ICollection<GamesOrders> GamesOrders { get; set; } = new List<GamesOrders>();

    }
}
