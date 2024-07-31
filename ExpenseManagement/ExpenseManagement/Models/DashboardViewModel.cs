namespace ExpenseManagement.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<ExpenseResponse> Expenses { get; set; }
        public IEnumerable<IncomeResponse> Income { get; set; }
    }
}
