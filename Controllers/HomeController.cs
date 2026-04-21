using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserApp.Data;
using UserApp.ViewModels;

namespace UserApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Tickets
                .Include(t => t.User)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            // ADMIN
            if (User.IsInRole("Admin"))
            {
                // voit tout
            }

            // AGENT
            else if (User.IsInRole("Agent"))
            {
                query = query.Where(t => t.AssignedToId == currentUserId);
            }

            // USER 
            else
            {
                query = query.Where(t => t.UserId == currentUserId);
            }

            var tickets = await query.ToListAsync();

            var model = new DashboardViewModel
            {
                TotalTickets = tickets.Count,
                OpenTickets = tickets.Count(x => x.Status == "Open"),
                InProgressTickets = tickets.Count(x => x.Status == "In Progress"),
                ClosedTickets = tickets.Count(x => x.Status == "Closed"),
                UrgentTickets = tickets.Count(x => x.Priority == "High"),

                SlaSuccessRate = tickets.Count > 0
                    ? (double)tickets.Count(x => x.Status == "Closed") / tickets.Count * 100
                    : 0,

                LatestTickets = tickets
                    .OrderByDescending(x => x.Id)
                    .Take(5)
                    .ToList(),

                Last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Now.AddDays(-i).ToString("dd/MM"))
                    .Reverse()
                    .ToList(),

                TicketsPerDay = Enumerable.Range(0, 7)
                    .Select(i => tickets.Count(t =>
                        t.CreatedAt.Date == DateTime.Now.AddDays(-i).Date))
                    .Reverse()
                    .ToList(),

                AgentStats = tickets
                    .Where(t => t.AssignedTo != null)
                    .GroupBy(t => t.AssignedTo.FullName)
                    .Select(g => new AgentStatsViewModel
                    {
                        AgentName = g.Key,
                        TicketCount = g.Count()
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}