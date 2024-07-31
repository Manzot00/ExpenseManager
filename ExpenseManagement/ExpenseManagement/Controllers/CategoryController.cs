using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Models;
using System.Diagnostics;
using ExpenseManagement.Filters;

namespace ExpenseManagement.Controllers
{
	[CustomeAuthorize]
	public class CategoryController : Controller
    {
        private readonly IExpenseManagementAPIClient _apiClient;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IExpenseManagementAPIClient apiClient, ILogger<CategoryController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Category";

            var userId = HttpContext.Session.GetString("UserId");

            try
            { 
                var response = _apiClient.GetAllUserCategoriesAsync(userId).Result;

				var expenseCategories = response.Where(c => c.Type == "Expense").ToList();
				var incomeCategories = response.Where(c => c.Type == "Income").ToList();

				var model = new CategoryViewModel
				{
					ExpenseCategories = expenseCategories,
					IncomeCategories = incomeCategories
				};

                return View(model);
			}
            catch(Exception ex)
            {
                _logger.LogError($"Error retrieving data: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(AddCategoryModel model)
		{
			if (ModelState.IsValid)
			{
				var userId = HttpContext.Session.GetString("UserId");

				var category = new UserCategoryDto
				{
					Name = model.Name,
					Type = model.Type,
					UserId = userId
				};

				try
				{
					await _apiClient.CreateUserCategoryAsync(category);
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error adding category: {ex.Message}");
					return StatusCode(500, "Internal server error");
				}
			}

			return View("Index");
		}

		[HttpGet]
		public async Task<IActionResult> GetCategory(string id)
		{
			try
			{
				var category = await _apiClient.GetUserCategoryAsync(id);

				return Json(category);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error fetching transaction: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpPost]
		public async Task<IActionResult> EditCategory(EditCategoryModel categoryModel)
		{
			if (ModelState.IsValid)
			{
				var userId = HttpContext.Session.GetString("UserId");
				_logger.LogInformation($"category ID: {categoryModel.Id}");

				try
				{
					var category = new UserCategoryDto
					{
						Name = categoryModel.Name,
						Type = categoryModel.Type,
						UserId = userId
					};
					_logger.LogInformation($"category: {Newtonsoft.Json.JsonConvert.SerializeObject(category)}");
					var response = await _apiClient.EditUserCategoryAsync(categoryModel.Id, category);

					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error editing category: {ex.Message}");
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
				return RedirectToAction("Index");
			}
		}

		[HttpPost]
		public async Task<IActionResult> DeleteCategory(DeleteModel deleteModel)
		{
			try
			{
				await _apiClient.DeleteUserCategoryAsync(deleteModel.Id);

				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error deleting category: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}
		}
    }
}
