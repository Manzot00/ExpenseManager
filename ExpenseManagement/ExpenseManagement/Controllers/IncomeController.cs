using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Models;
using ExpenseManagement.Filters;


namespace ExpenseManagement.Controllers
{
	[CustomeAuthorize]
	public class IncomeController : Controller
    {
        private readonly IExpenseManagementAPIClient _apiClient;
        private readonly ILogger<DashboardController> _logger;

        public IncomeController(IExpenseManagementAPIClient apiClient, ILogger<DashboardController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateIncome(AddTransactionModel incomeModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var userId = HttpContext.Session.GetString("UserId");

            try
            {
                var income = new IncomeDto
                {
                    Name = incomeModel.Name,
                    Amount = incomeModel.Amount,
                    Date = incomeModel.Date.AddHours(3),
                    UserId = userId,
                    CategoryId = incomeModel.CategoryId,
                    Note = incomeModel.Note
                };

                var response = await _apiClient.CreateIncomeAsync(income);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating income: " + ex.Message);
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetIncome(string id)
        {
            try
            {
                var income = await _apiClient.GetIncomeAsync(id);

                return Json(income);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching transaction: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditIncome(EditTransactionModel incomeModel)
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
                var income = new ExpenseDto
                {
                    Name = incomeModel.Name,
                    Amount = incomeModel.Amount,
                    Date = incomeModel.Date.AddHours(3),
                    UserId = userId,
                    CategoryId = incomeModel.CategoryId,
                    Note = incomeModel.Note
                };

                var response = await _apiClient.EditExpenseAsync(incomeModel.Id, income);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error editing expense: " + ex.Message);
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIncome(DeleteModel deleteIncome)
        {
            try
            {
                await _apiClient.DeleteIncomeAsync(deleteIncome.Id);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error deleting income: " + ex.Message);
                return RedirectToAction("Index", "Dashboard");
            }
        }

    }
}
