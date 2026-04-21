namespace UserApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public List<Ticket>? Tickets { get; set; }
    }
}
