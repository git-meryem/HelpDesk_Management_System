  namespace UserApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public string? UserId { get; set; }
        public Users? User { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
