
namespace GamesForU.Models
{
    public class Pg
    {
        public int Id { get; set; }
        public int AgeRestriction { get; set; }

        public virtual ICollection<Games> Games { get; set; } = new List<Games>();
    }
}
