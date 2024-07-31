using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models
{
    public class EditTransactionModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public float Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string CategoryId { get; set; }

        public string? Note { get; set; }
    }
}
