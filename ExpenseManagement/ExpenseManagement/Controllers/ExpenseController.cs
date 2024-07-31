using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Models;
using Newtonsoft.Json;
using ExpenseManagement.Filters;

namespace ExpenseManagement.Controllers
{
	[CustomeAuthorize]
	public class ExpenseController : Controller
    {
        private readonly IExpenseManagementAPIClient _apiClient;
        private readonly ILogger<DashboardController> _logger;

        public ExpenseController(IExpenseManagementAPIClient apiClient, ILogger<DashboardController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense(AddTransactionModel expenseModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is not valid.");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("ModelState Error: {ErrorMessage}", error.ErrorMessage);
                    }
                }
                return RedirectToAction("Index", "Dashboard");
            }

            var userId = HttpContext.Session.GetString("UserId");
            _logger.LogInformation(expenseModel.Name, expenseModel.Amount, expenseModel.Date, expenseModel.CategoryId, expenseModel.Note);
            _logger.LogInformation(userId);
            try
            {
                var expense = new ExpenseDto
                {
                    Name = expenseModel.Name,
                    Amount = expenseModel.Amount,
                    Date = expenseModel.Date.AddHours(3),
                    UserId = userId,
                    CategoryId = expenseModel.CategoryId,
                    Note = expenseModel.Note
                };

                var response = await _apiClient.CreateExpenseAsync(expense);
                _logger.LogInformation($"Created expense: {response}");

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating expense: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExpense(string id)
        {
            if (id == null)
			{
				return StatusCode(400, "Bad Request");
			}

            try 
            { 
                var expense = await _apiClient.GetExpenseAsync(id);

                return Json(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching transaction: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditExpense(EditTransactionModel expenseModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is not valid.");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("ModelState Error: {ErrorMessage}", error.ErrorMessage);
                    }
                }
                return RedirectToAction("Index", "Dashboard");
            }

            var userId = HttpContext.Session.GetString("UserId");

            try
            {
                var expense = new ExpenseDto
                {
                    Name = expenseModel.Name,
                    Amount = expenseModel.Amount,
                    Date = expenseModel.Date.AddHours(3),
                    UserId = userId,
                    CategoryId = expenseModel.CategoryId,
                    Note = expenseModel.Note
                };

                var response = await _apiClient.EditExpenseAsync(expenseModel.Id, expense);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error editing expense: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExpense(DeleteModel deleteExpense)
        {
            try
            {
                await _apiClient.DeleteExpenseAsync(deleteExpense.Id);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting transaction: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
