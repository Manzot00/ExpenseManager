namespace ExpenseManagement.Models
{
    public class AdminViewModel
    {
        public int Total { get; set; }
        public int TotalAdmin { get; set; }
        public int TotalUser { get; set; }
        public int TotalCategories { get; set; }
        public IEnumerable<GlobalCategoryResponse> ExpenseGlobalCategories { get; set; }
        public IEnumerable<GlobalCategoryResponse> IncomeGlobalCategories { get; set; }
    }
}
