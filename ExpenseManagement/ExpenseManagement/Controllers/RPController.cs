using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Models;
using ExpenseManagement.Filters;

namespace ExpenseManagement.Controllers
{
	[CustomeAuthorize]
	public class RPController : Controller
	{
		private readonly IExpenseManagementAPIClient _apiClient;
		private readonly ILogger<RPController> _logger;

		public RPController(IExpenseManagementAPIClient apiClient, ILogger<RPController> logger)
		{
			_apiClient = apiClient;
			_logger = logger;
		}
		public IActionResult Index()
		{
			ViewData["ActivePage"] = "RegularPayments";

			var userId = HttpContext.Session.GetString("UserId");

			try
			{
				var rps = _apiClient.GetAllRPsAsync(userId).Result;
				var categories = _apiClient.GetAllUserCategoriesAsync(userId).Result;

				var expenseRP = rps.Where(c => c.Type == "Expense").ToList();
				var incomeRP = rps.Where(c => c.Type == "Income").ToList();

				var model = new RegularPaymentsViewModel
				{
					ExpenseRPs = expenseRP,
					IncomeRPs = incomeRP
				};

				var expenseCategories = categories.Where(c => c.Type == "Expense").ToList();
				var incomeCategories = categories.Where(c => c.Type == "Income").ToList();

				ViewBag.ExpenseCategories = expenseCategories;
				ViewBag.IncomeCategories = incomeCategories;

				return View(model);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error retrieving data: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpPost]
		public async Task<IActionResult> CreateRP(AddRegularPaymentModel rpModel)
		{
			if (ModelState.IsValid)
			{
				var userId = HttpContext.Session.GetString("UserId");
				try
				{
					var rp = new RegularPaymentDto
					{
						Name = rpModel.Name,
						Type = rpModel.Type,
						Amount = rpModel.Amount,
						CategoryId = rpModel.CategoryId,
						UserId = userId,
						RecurrenceDay = rpModel.RecurrenceDay,
						Note = rpModel.Note
					};

					var response = await _apiClient.CreateRPAsync(rp);

					return RedirectToAction("Index");

				}
				catch (Exception ex)
				{
					_logger.LogError($"Error adding Regular Payment {rpModel.Name}: {ex.Message}");
					return StatusCode(500, "Internal server error");
				}
			}
			else
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
		}

		[HttpGet]
		public async Task<IActionResult> GetRP(string id)
		{ 
			if (id == null)
			{
				return StatusCode(400, "Bad Request");
			}

			try
			{
				var rp = await _apiClient.GetRPAsync(id);

				return Json(rp);
			}
			catch(Exception ex)
			{
				_logger.LogError($"Error fetching Regular Payment: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpPost]
		public async Task<IActionResult> EditRP(EditRegularPaymentModel rpModel)
		{
			if (ModelState.IsValid)
			{
				var userId = HttpContext.Session.GetString("UserId");
				try
				{
					var rp = new RegularPaymentDto
					{
						Name = rpModel.Name,
						Type = rpModel.Type,
						Amount = rpModel.Amount,
						CategoryId = rpModel.CategoryId,
						UserId = userId,
						RecurrenceDay = rpModel.RecurrenceDay,
						Note = rpModel.Note
					};

					var response = await _apiClient.UpdateRPAsync(rpModel.Id, rp);

					return RedirectToAction("Index");

				}
				catch (Exception ex)
				{
					_logger.LogError($"Error editing Regular Payment {rpModel.Name}: {ex.Message}");
					return StatusCode(500, "Internal server error");
				}
			}
			else
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
		}

		[HttpPost]
		public async Task<IActionResult> DeleteRP(DeleteModel deleteRP)
		{
			try
			{
				await _apiClient.DeleteRPAsync(deleteRP.Id);

				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error deleting Regular Payment {deleteRP.Id}: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}
		}
	}
}
