namespace GamesForU.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Games> Games { get; set; } = new List<Games>();

    }
}
