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
    public class ExpenseController : ControllerBase
    {
        private readonly Client _supabaseClient;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(Client supabaseClient, ILogger<ExpenseController> logger)
        {
            _supabaseClient = supabaseClient;
            _logger = logger;
        }

        [HttpPost("createExpense")]
        public async Task<ActionResult<Expense>> CreateExpense(ExpenseDto expenseDto)
        {
            try
            {
                var expense = new Expense
                {
                    Name = expenseDto.Name,
                    Amount = expenseDto.Amount,
                    Date = expenseDto.Date,
                    UserId = expenseDto.UserId,
                    CategoryId = expenseDto.CategoryId,
                    Note = expenseDto.Note
                };

                // Insert the expense into the database
                var response = await _supabaseClient.From<Expense>().Insert(expense);
                var createdExpense = response.Models[0];

                return Ok(new 
                { 
                    Id = createdExpense.Id.ToString(),
                    Name = createdExpense.Name,
                    Amount = createdExpense.Amount,
                    Date = createdExpense.Date,
                    UserId = createdExpense.UserId.ToString(),
                    CategoryId = createdExpense.CategoryId.ToString(),
                    Note = createdExpense.Note
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getAllExpenses/{userId}")]
        public async Task<ActionResult<IEnumerable<ExpenseResponse>>> GetExpenses(string userId)
        {
            try
            {

                var expensesResponse = await _supabaseClient
                    .From<Expense>()
                    .Filter("user_id", Constants.Operator.Equals, userId)
                    .Get();

                var CategoryResponse = await _supabaseClient
                    .From<UserCategory>()
                    .Filter("user_id", Constants.Operator.Equals, userId)
                    .Get();

                var expenses = expensesResponse.Models.Select(expense => new ExpenseResponse
                {
                    Id = expense.Id.ToString(),
                    Name = expense.Name,
                    Amount = expense.Amount,
                    Date = expense.Date.AddHours(3),
                    UserId = expense.UserId.ToString(),
                    CategoryId = expense.CategoryId.ToString(),
                    CategoryName = CategoryResponse.Models.FirstOrDefault(c => c.Id == expense.CategoryId)?.Name,
                    Note = expense.Note
                }).ToList();

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getExpense/{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(string id)
        {
            try
            {
                var response = await _supabaseClient
                    .From<Expense>()
                    .Filter("id", Constants.Operator.Equals, id)
                    .Get();

                if (response.Models.Count == 0)
                {
                    return NotFound(new { message = "Expense not found" });
                }

                var expense = response.Models[0];

                return Ok(new ExpenseDto
                {
                    Name = expense.Name,
                    Amount = expense.Amount,
                    Date = expense.Date.AddHours(3),
                    UserId = expense.UserId.ToString(),
                    CategoryId = expense.CategoryId.ToString(),
                    Note = expense.Note
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("editExpense/{id}")]
        public async Task<ActionResult<Expense>> EditExpense(ExpenseDto expenseDto, string id)
        {
            try
            {
                var expense = new Expense
                {
                    Id = id,
                    Name = expenseDto.Name,
                    Amount = expenseDto.Amount,
                    Date = expenseDto.Date,
                    UserId = expenseDto.UserId,
                    CategoryId = expenseDto.CategoryId,
                    Note = expenseDto.Note
                };

                // Salva le modifiche nel database
                var response = await _supabaseClient
                    .From<Expense>()
                    .Update(expense);

                var updatedExpense = response.Models[0];

                return Ok(new
                {
                    Id = updatedExpense.Id.ToString(),
                    Name = updatedExpense.Name,
                    Amount = updatedExpense.Amount,
                    Date = updatedExpense.Date,
                    UserId = updatedExpense.UserId.ToString(),
                    CategoryId = updatedExpense.CategoryId.ToString(),
                    Note = updatedExpense.Note
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("deleteExpense/{id}")]
        public async Task<ActionResult> DeleteExpense(string id)
        {
            try
            {
                // Tenta di eliminare la spesa direttamente
                await _supabaseClient
                    .From<Expense>()
                    .Filter("id", Constants.Operator.Equals, id)
                    .Delete();

                return Ok(new { message = "Expense (" + id + ") deleted"});
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }

    public class ExpenseDto
    { 
        public string Name { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public string CategoryId { get; set; }
        public string? Note { get; set; }
    }

    public class ExpenseResponse
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
