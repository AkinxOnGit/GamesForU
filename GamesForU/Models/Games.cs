using System.ComponentModel.DataAnnotations.Schema;

namespace GamesForU.Models
{
    public class Games
    {
        public int Id { get; set; }

        public string Price { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; }

        [ForeignKey("Publisher")]
        public int PublisherId { get; set; }
        public virtual Publisher? Publisher { get; set; }


        [ForeignKey("Pg")]
        public int PgId { get; set; }
        public virtual Pg? Pg { get; set; }


        public virtual ICollection<Category> Category { get; set; } = new List<Category>();
        public virtual ICollection<GamesOrders> GamesOrders { get; set; } = new List<GamesOrders>();
    }
}
