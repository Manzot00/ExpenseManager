namespace ExpenseManagement.Models
{
    public class CategoryViewModel
    {
		public IEnumerable<UserCategoryResponse> ExpenseCategories { get; set; }
		public IEnumerable<UserCategoryResponse> IncomeCategories { get; set; }
	}
}
