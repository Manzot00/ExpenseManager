using ExpenseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExpenseManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IExpenseManagementAPIClient _apiClient;
		private readonly ILogger<DashboardController> _logger;

		public HomeController(IExpenseManagementAPIClient apiClient, ILogger<DashboardController> logger)
		{
			_apiClient = apiClient;
			_logger = logger;
		}

		public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the errors and try again." });
            }

            try
            {
                var loginDto = new LoginDto
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var response = await _apiClient.LoginAsync(loginDto);
                _logger.LogInformation($"Role: {response.Role}");
				// Salva i dati dell'utente e i token nella sessione

				HttpContext.Session.SetString("Username", response.Username);
				HttpContext.Session.SetString("UserId", response.UserId);
                HttpContext.Session.SetString("AccessToken", response.AccessToken);
                HttpContext.Session.SetString("RefreshToken", response.RefreshToken);
                HttpContext.Session.SetString("Role", response.Role);

                if (response.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Dashboard");
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 401) // Unauthorized
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                }
                else
                {
                    // Restituisci un messaggio di errore generico per altri tipi di eccezioni
                    ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                }

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the errors and try again." });
            }

            try
            {
                var registerDto = new RegisterDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    Username = model.Username
                };

                var response = await _apiClient.RegisterAsync(registerDto);
                

                // Salva i dati dell'utente e i token nella sessione
                HttpContext.Session.SetString("Username", response.Username);
                HttpContext.Session.SetString("UserId", response.UserId);
                HttpContext.Session.SetString("AccessToken", response.AccessToken);
                HttpContext.Session.SetString("RefreshToken", response.RefreshToken);
				HttpContext.Session.SetString("Role", response.Role);

				return RedirectToAction("Index", "Dashboard");
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");

                return View(model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
