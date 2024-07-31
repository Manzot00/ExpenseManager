using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpensesManagementAPI.Models
{
    [Table("user_category")]
    public class UserCategory : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }
    }
}
