

using System.Net.Mail;

namespace UserApp.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? Status { get; set; }
        public string? Priority { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? UserId { get; set; }
        public Users? User { get; set; }

        public string? AssignedToId { get; set; }
        public Users? AssignedTo { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public List<Comment>? Comments { get; set; }

        public List<Attachment>? Attachments { get; set; }
    }
}
