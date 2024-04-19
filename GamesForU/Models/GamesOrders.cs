using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using GamesForU.Models;

namespace GamesForU.Models
{
    public class GamesOrders
    {
        public GamesOrders(int amount,int gamesId,int ordersId)
        {
            Amount = amount;
            GamesId = gamesId;
            OrdersId = ordersId;
        }
        public int Id { get; set; }
        public int Amount { get; set; }


        [ForeignKey("Games")]
        public int GamesId { get; set; }
        public virtual Games? Games { get; set; }



        [ForeignKey("Orders")]
        public int OrdersId { get; set; }
        public virtual Orders? Orders { get; set; }

    }
}
