using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpensesManagementAPI.Models
{
    [Table("global_category")]
    public class GlobalCategory : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("type")]
        public string Type { get; set; }
    }
}
