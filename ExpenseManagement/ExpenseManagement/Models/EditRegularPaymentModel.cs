using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models
{
	public class EditRegularPaymentModel
	{
		public string Id { get; set; }

		[Required(ErrorMessage = "Name is required")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Amount is required")]
		[Range(0.01, float.MaxValue, ErrorMessage = "Amount must be greater than zero")]
		public float Amount { get; set; }

		[Required(ErrorMessage = "Recurrence Day is required")]
		[Range(1, 31, ErrorMessage = "Recurrence Day must be between 1 and 31")]
		public int RecurrenceDay { get; set; }

		[Required(ErrorMessage = "Category is required")]
		public string CategoryId { get; set; }

		[Required(ErrorMessage = "Type is required")]
		public string Type { get; set; }
		public string? Note { get; set; }
	}
}
