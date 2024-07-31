namespace ExpenseManagement.Models
{
	public class RegularPaymentsViewModel
	{
		public IEnumerable<RegularPaymentResponse> ExpenseRPs { get; set; }
		public IEnumerable<RegularPaymentResponse> IncomeRPs { get; set; }
	}
}
