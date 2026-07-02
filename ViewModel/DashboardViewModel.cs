using Sabara.Models;
using Sabara.Web.Models;

namespace Sabara.Web.ViewModel
{
    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int RepliedOrders { get; set; }
        public int OrdersToday { get; set; }
        public int OrdersThisWeek { get; set; }
        public int OrdersThisMonth { get; set; }

        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int LeadClients { get; set; }
        public int ClientsThisMonth { get; set; }
        public int WonOrders { get; set; }
        public int LostOrders { get; set; }
        public int ConversionPercent { get; set; }

        public List<Project> RecentProjects { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
        public List<Client> RecentClients { get; set; } = new();

        public List<CategorySlice> ProjectsByCategory { get; set; } = new();
        public List<DayPoint> OrdersLast7Days { get; set; } = new();
        public List<StageSlice> OrdersByStage { get; set; } = new();
    }

    public class StageSlice
    {
        public OrderStage Stage { get; set; }
        public int Count { get; set; }
    }

    public class CategorySlice
    {
        public string Category { get; set; } = "";
        public int Count { get; set; }
    }

    public class DayPoint
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string Label => Date.ToString("dd/MM");
    }
}
