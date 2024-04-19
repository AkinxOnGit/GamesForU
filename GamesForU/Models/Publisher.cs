namespace GamesForU.Models
{
    public class Publisher
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Games> Games { get; set; } = new List<Games>();

    }
}
