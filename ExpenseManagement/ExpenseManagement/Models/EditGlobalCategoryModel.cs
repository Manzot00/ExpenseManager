using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models
{
	public class EditGlobalCategoryModel
	{
		public string Id { get; set; }

		[Required(ErrorMessage = "Name is required")]
		public string Name { get; set; }

		public string OldName { get; set; }

		[Required(ErrorMessage = "Type is required")]
		public string Type { get; set; }

		public string OldType { get; set; }
	}
}
