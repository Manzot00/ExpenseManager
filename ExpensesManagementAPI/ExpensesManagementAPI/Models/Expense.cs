using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpensesManagementAPI.Models
{
    [Table("expenses")]
    public class Expense : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("category_id")]
        public string CategoryId { get; set; }

        [Column("amount")]
        public float Amount { get; set; }

        [Column("note")]
        public string? Note { get; set; }
    }
}
