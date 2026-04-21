using Microsoft.AspNetCore.Identity;

namespace UserApp.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public List<Ticket> CreatedTickets { get; set; } = new();
        public List<Ticket> AssignedTickets { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public bool IsDeleted { get; set; } = false;

        public string? ProfileImage { get; set; }
    }
}