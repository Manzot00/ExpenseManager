using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Client = Supabase.Client;
using Supabase.Postgrest;
using ExpensesManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExpensesManagementAPI.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class RPController : ControllerBase
	{
		private readonly Client _supabaseClient;
		private readonly ILogger<RPController> _logger;

		public RPController(Client supabaseClient, ILogger<RPController> logger)
		{
			_supabaseClient = supabaseClient;
			_logger = logger;
		}

		[HttpPost("createRP")]
		public async Task<ActionResult<RegularPayment>> CreateRP(RegularPaymentDto rpDto)
		{
			try
			{
				var rp = new RegularPayment
				{
					Name = rpDto.Name,
					Amount = rpDto.Amount,
					RecurrenceDay = rpDto.RecurrenceDay,
					UserId = rpDto.UserId,
					CategoryId = rpDto.CategoryId,
					Type = rpDto.Type,
					Note = rpDto.Note
				};

				// Insert the rp into the database
				var response = await _supabaseClient.From<RegularPayment>().Insert(rp);
				var createdRP = response.Models[0];

				return Ok(new {
					Id = createdRP.Id.ToString(),
					Name = createdRP.Name,
					Amount = createdRP.Amount,
					RecurrenceDay = createdRP.RecurrenceDay,
					UserId = createdRP.UserId.ToString(),
					CategoryId = createdRP.CategoryId.ToString(),
					Type = createdRP.Type,
					Note = createdRP.Note
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("getAllRPs/{userId}")]
		public async Task<ActionResult<IEnumerable<RegularPaymentResponse>>> GetRPs(string userId)
		{
			try
			{
				var response = await _supabaseClient
					.From<RegularPayment>()
					.Filter("user_id", Constants.Operator.Equals, userId.ToString())
					.Get();

				var categoryResponse = await _supabaseClient
					.From<UserCategory>()
					.Filter("user_id", Constants.Operator.Equals, userId)
					.Get();

				var rps = response.Models.Select(rp => new RegularPaymentResponse
				{
					Id = rp.Id.ToString(),
					Name = rp.Name,
					Amount = rp.Amount,
					RecurrenceDay = rp.RecurrenceDay,
					UserId = rp.UserId,
					CategoryId = rp.CategoryId,
					CategoryName = categoryResponse.Models.FirstOrDefault(c => c.Id == rp.CategoryId)?.Name,
					Type = rp.Type,
					Note = rp.Note
				});

				return Ok(rps);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("getRP/{id}")]
		public async Task<ActionResult<RegularPaymentResponse>> GetRP(string id)
		{
			try
			{
				var response = await _supabaseClient
					.From<RegularPayment>()
					.Get();

				var categoryResponse = await _supabaseClient
					.From<UserCategory>()
					.Get();

				var rp = response.Models.FirstOrDefault(rp => rp.Id == id);

				if (rp == null)
				{
					return NotFound();
				}

				var rpResponse = new RegularPaymentResponse
				{
					Id = rp.Id.ToString(),
					Name = rp.Name,
					Amount = rp.Amount,
					RecurrenceDay = rp.RecurrenceDay,
					UserId = rp.UserId,
					CategoryId = rp.CategoryId,
					CategoryName = categoryResponse.Models.FirstOrDefault(c => c.Id == rp.CategoryId)?.Name,
					Type = rp.Type,
					Note = rp.Note
				};

				return Ok(rpResponse);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("getAllUsersRPs")]
		public async Task<ActionResult<IEnumerable<RegularPaymentResponse>>> GetAllRPs()
		{
			try
			{
				var response = await _supabaseClient
					.From<RegularPayment>()
					.Get();

				var categoryResponse = await _supabaseClient
					.From<UserCategory>()
					.Get();

				var rps = response.Models.Select(rp => new RegularPaymentResponse
				{
					Id = rp.Id.ToString(),
					Name = rp.Name,
					Amount = rp.Amount,
					RecurrenceDay = rp.RecurrenceDay,
					UserId = rp.UserId,
					CategoryId = rp.CategoryId,
					CategoryName = categoryResponse.Models.FirstOrDefault(c => c.Id == rp.CategoryId)?.Name,
					Type = rp.Type,
					Note = rp.Note
				});

				return Ok(rps);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPut("updateRP/{id}")]
		public async Task<ActionResult<RegularPayment>> UpdateRP(string id, RegularPaymentDto rpDto)
		{
			try
			{
				var rp = new RegularPayment
				{
					Id = id,
					Name = rpDto.Name,
					Amount = rpDto.Amount,
					RecurrenceDay = rpDto.RecurrenceDay,
					UserId = rpDto.UserId,
					CategoryId = rpDto.CategoryId,
					Type = rpDto.Type,
					Note = rpDto.Note
				};

				var response = await _supabaseClient.From<RegularPayment>().Update(rp);
				var updatedRP = response.Models[0];

				return Ok(new {
					Id = updatedRP.Id.ToString(),
					Name = updatedRP.Name,
					Amount = updatedRP.Amount,
					RecurrenceDay = updatedRP.RecurrenceDay,
					UserId = updatedRP.UserId.ToString(),
					CategoryId = updatedRP.CategoryId.ToString(),
					Type = updatedRP.Type,
					Note = updatedRP.Note
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpDelete("deleteRP/{id}")]
		public async Task<ActionResult> DeleteRP(string id)
		{
			try
			{
				await _supabaseClient
					.From<RegularPayment>()
					.Filter("id", Constants.Operator.Equals, id)
					.Delete();

				return Ok(new { message = "RegularPayment (" + id + ") deleted" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}

	public class RegularPaymentDto 
	{ 
		public string Name { get; set; }
		public float Amount { get; set; }
		public int RecurrenceDay { get; set; }
		public string UserId { get; set; }
		public string CategoryId { get; set; }
		public string Type { get; set; }
		public string? Note { get; set; }

	}

	public class RegularPaymentResponse
	{ 
		public string Id { get; set; }
		public string Name { get; set; }
		public float Amount { get; set; }
		public int RecurrenceDay { get; set; }
		public string UserId { get; set; }
		public string CategoryId { get; set; }
		public string CategoryName { get; set; }
		public string Type { get; set; }
		public string? Note { get; set; }
	}
}
