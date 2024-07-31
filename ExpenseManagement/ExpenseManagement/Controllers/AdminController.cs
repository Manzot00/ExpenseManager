using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Models;
using ExpenseManagement.Filters;

namespace ExpenseManagement.Controllers
{
    [CustomeAuthorize]
    public class AdminController : Controller //Controller per gestire la dashboard Admin e le categorie globali
    {
        private readonly IExpenseManagementAPIClient _apiClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IExpenseManagementAPIClient apiClient, ILogger<AdminController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Admin";

            try
            {
                var totals = _apiClient.TotalUsersAsync().Result;

                var total = totals.Total;
                var totalAdmins = totals.TotalAdmin;
                var totalUsers = totals.TotalUser;

                var globalCategories = _apiClient.GetAllGlobalCategoriesAsync().Result;
                var incomeCategories = globalCategories.Where(c => c.Type == "Income").ToList();
                var expenseCategories = globalCategories.Where(c => c.Type == "Expense").ToList();

                var model = new AdminViewModel
                {
                    Total = total,
                    TotalAdmin = totalAdmins,
                    TotalUser = totalUsers,
                    TotalCategories = globalCategories.Count(),
                    ExpenseGlobalCategories = expenseCategories,
                    IncomeGlobalCategories = incomeCategories
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving data: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
		public async Task<IActionResult> CreateGlobalCategory(AddCategoryModel model)
		{
			if (ModelState.IsValid)
			{

				var category = new GlobalCategoryDto
				{
					Name = model.Name,
					Type = model.Type,
				};

				try
				{
					await _apiClient.CreateGlobalCategoryAsync(category);

					// prende tutti gli id degli utenti e crea una categoria per ciascuno
					var userIds = await _apiClient.GetUserIdsAsync();
					foreach (var userId in userIds)
					{
						var userCategory = new UserCategoryDto
						{
							Name = model.Name,
							Type = model.Type,
							UserId = userId
						};
						
						await _apiClient.CreateUserCategoryAsync(userCategory);
					}
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error adding category: {ex.Message}");
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

		[HttpGet]
		public async Task<IActionResult> GetGlobalCategory(string id)
		{
			try
			{
				var category = await _apiClient.GetGlobalCategoryAsync(id);

				return Json(category);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error fetching transaction: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpPost]
		public async Task<IActionResult> EditGlobalCategory(EditGlobalCategoryModel categoryModel)
		{
			if (ModelState.IsValid)
			{
				_logger.LogInformation($"category ID: {categoryModel.Id}");

				try
				{
					var category = new GlobalCategoryDto
					{
						Name = categoryModel.Name,
						Type = categoryModel.Type,
					};
					_logger.LogInformation($"category: {Newtonsoft.Json.JsonConvert.SerializeObject(category)}");
					var response = await _apiClient.EditGlobalCategoryAsync(categoryModel.Id, category);

					// prende tutte le categorie utente con il vecchio nome e tipo e le modifica
					var userCategories = await _apiClient.GetUserCategoryByNameTypeAsync(categoryModel.OldType, categoryModel.OldName);

					foreach (var userCategory in userCategories)
					{
						var userCategoryDto = new UserCategoryDto
						{
							Name = categoryModel.Name,
							Type = categoryModel.Type,
							UserId = userCategory.UserId
						};

						await _apiClient.EditUserCategoryAsync(userCategory.Id, userCategoryDto);
					}

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
		public async Task<IActionResult> DeleteGlobalCategory(DeleteGlobalCategoryModel deleteModel)
		{
			try
			{
				await _apiClient.DeleteGlobalCategoryAsync(deleteModel.Id);

				// prende tutte le categorie utente con il nome e il tipo e le elimina
				var userCategories = await _apiClient.GetUserCategoryByNameTypeAsync(deleteModel.Type, deleteModel.Name);

				foreach (var userCategory in userCategories) {
					await _apiClient.DeleteUserCategoryAsync(userCategory.Id);
				}

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
