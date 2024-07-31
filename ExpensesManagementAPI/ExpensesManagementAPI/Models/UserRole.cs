using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpensesManagementAPI.Models
{
	[Table("user_role")]
	public class UserRole : BaseModel
	{
		[PrimaryKey("id", false)]
		public string Id { get; set; }

		[Column("user_id")]
		public string UserId { get; set; }

		[Column("role")]
		public string Role { get; set; }
	}
}
