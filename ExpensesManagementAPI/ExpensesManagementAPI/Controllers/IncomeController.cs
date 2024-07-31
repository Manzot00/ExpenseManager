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
    public class IncomeController : ControllerBase
    {
        private readonly Client _supabaseClient;

        public IncomeController(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        [HttpPost("createIncome")]
        public async Task<ActionResult<Income>> CreateIncome(IncomeDto incomeDto)
        {
            try
            {
                var income = new Income
                {
                    Name = incomeDto.Name,
                    Amount = incomeDto.Amount,
                    Date = incomeDto.Date,
                    UserId = incomeDto.UserId,
                    CategoryId = incomeDto.CategoryId,
                    Note = incomeDto.Note
                };

                // Insert the income into the database
                var response = await _supabaseClient.From<Income>().Insert(income);
                var createdIncome = response.Models[0];

                return Ok(new
                {
                    Id = createdIncome.Id,
                    Name = createdIncome.Name,
                    Amount = createdIncome.Amount,
                    Date = createdIncome.Date,
                    UserId = createdIncome.UserId,
                    CategoryId = createdIncome.CategoryId,
                    Note = createdIncome.Note
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getAllIncomes/{userId}")]
        public async Task<ActionResult<IEnumerable<IncomeResponse>>> GetIncomes(string userId)
        {
            try
            {
                var IncomeResponse = await _supabaseClient
                    .From<Income>()
                    .Filter("user_id", Constants.Operator.Equals, userId)
                    .Get();

                var CategoryResponse = await _supabaseClient
                   .From<UserCategory>()
                   .Filter("user_id", Constants.Operator.Equals, userId)
                   .Get();

                var incomes = IncomeResponse.Models.Select(income => new IncomeResponse
                {
                    Id = income.Id,
                    Name = income.Name,
                    Amount = income.Amount,
                    Date = income.Date.AddHours(3),
                    UserId = income.UserId,
                    CategoryId = income.CategoryId,
                    CategoryName = CategoryResponse.Models.FirstOrDefault(category => category.Id == income.CategoryId)?.Name,
                    Note = income.Note
                }).ToList();

                return Ok(incomes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getIncome/{id}")]
        public async Task<ActionResult<IncomeDto>> GetIncome(string id)
        {
            try
            {
                var response = await _supabaseClient
                    .From<Income>()
                    .Filter("id", Constants.Operator.Equals, id)
                    .Get();

                if (response.Models.Count == 0)
                {
                    return NotFound(new { message = "Income not found" });
                }

                var income = response.Models[0];

                return Ok(new IncomeDto
                {
                    Name = income.Name,
                    Amount = income.Amount,
                    Date = income.Date.AddHours(3),
                    UserId = income.UserId,
                    CategoryId = income.CategoryId,
                    Note = income.Note
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("editIncome/{id}")]
        public async Task<ActionResult<Income>> EditIncome(IncomeDto incomeDto, string id)
        {
            try
            {
                var income = new Income
                {
                    Id = id,
                    Name = incomeDto.Name,
                    Amount = incomeDto.Amount,
                    Date = incomeDto.Date,
                    UserId = incomeDto.UserId,
                    CategoryId = incomeDto.CategoryId,
                    Note = incomeDto.Note
                };

                // Save the changes to the database
                var response = await _supabaseClient
                    .From<Income>()
                    .Update(income);

                var updatedIncome = response.Models[0];

                return Ok(new
                {
                    Id = updatedIncome.Id,
                    Name = updatedIncome.Name,
                    Amount = updatedIncome.Amount,
                    Date = updatedIncome.Date,
                    UserId = updatedIncome.UserId,
                    CategoryId = updatedIncome.CategoryId,
                    Note = updatedIncome.Note
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("deleteIncome/{id}")]
        public async Task<ActionResult> DeleteIncome(string id)
        {
            try
            {
                // Try to delete the income directly
                await _supabaseClient
                    .From<Income>()
                    .Filter("id", Constants.Operator.Equals, id)
                    .Delete();

                return Ok(new { message = "Income (" + id + ") deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class IncomeDto
    {
        public string Name { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public string CategoryId { get; set; }
        public string? Note { get; set; }
    }

    public class IncomeResponse
    { 
        public string Id { get; set; }
        public string Name { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Note { get; set; }
    }
}
