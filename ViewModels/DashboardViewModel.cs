using UserApp.Models;

namespace UserApp.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int UrgentTickets { get; set; }

        public double SlaSuccessRate { get; set; }

        public List<Ticket> LatestTickets { get; set; }

        public List<int> TicketsPerDay { get; set; }
        public List<string> Last7Days { get; set; }     

        public List<AgentStatsViewModel> AgentStats { get; set; }
    }
}
