using ExpenseManagement.Filters;
using ExpenseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ExpenseManagement.Controllers
{
    [CustomeAuthorize]
    public class DashboardController : Controller
    {
        private readonly IExpenseManagementAPIClient _apiClient;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IExpenseManagementAPIClient apiClient, ILogger<DashboardController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Dashboard";
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;

            var userId = HttpContext.Session.GetString("UserId");
            _logger.LogInformation($"User {username} with ID {userId} accessed the dashboard.");

            // Attendi i risultati delle chiamate asincrone
            try
            {
                var userExpensesTask = _apiClient.GetAllExpensesAsync(userId);
                var userIncomeTask = _apiClient.GetAllIncomesAsync(userId);
                var categoriesTask = _apiClient.GetAllUserCategoriesAsync(userId);

                await Task.WhenAll(userExpensesTask, userIncomeTask, categoriesTask);

                _logger.LogInformation($"Expenses [{userExpensesTask.Result.Count()}] and Income [{userIncomeTask.Result.Count()}] retrieved successfully.");

                var userExpenses = userExpensesTask.Result;
                var userIncome = userIncomeTask.Result;
                var categories = categoriesTask.Result; 

                var expenseCategories = categories.Where(c => c.Type == "Expense");
                var incomeCategories = categories.Where(c => c.Type == "Income");

                var model = new DashboardViewModel
                {
                    Expenses = userExpenses,
                    Income = userIncome,
                };

                ViewBag.ExpenseCategories = expenseCategories;
                ViewBag.IncomeCategories = incomeCategories;

                ViewBag.ExpenseCategoryData = userExpenses
                    .GroupBy(e => e.CategoryName)
                    .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                    .ToList();

                _logger.LogInformation($"Expense categories: {JsonConvert.SerializeObject(ViewBag.ExpenseCategoryData)}");

                ViewBag.IncomeCategoryData = userIncome
                    .GroupBy(i => i.CategoryName)
                    .Select(g => new { Category = g.Key, Total = g.Sum(i => i.Amount) })
                    .ToList();

                _logger.LogInformation($"Income categories: {JsonConvert.SerializeObject(ViewBag.IncomeCategoryData)}");

                return View(model);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving data: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            ViewData["ActivePage"] = "Logout";
            var accessToken = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                await _apiClient.LogoutAsync();

                // Clear the session
                HttpContext.Session.Clear();

                return RedirectToAction("Index", "Home");

            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 401) // Unauthorized
                {
                    return Json(new { success = false, message = "Unauthorized access. Please login again." });
                }

                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }
    }

}
