using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models
{
	public class AddCategoryModel
	{
		[Required(ErrorMessage = "Name is required")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Type is required")]
		public string Type { get; set; }
	}
}
