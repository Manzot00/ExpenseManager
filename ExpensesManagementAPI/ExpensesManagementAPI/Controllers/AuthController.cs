using Microsoft.AspNetCore.Mvc;
using Supabase.Gotrue;
using Client = Supabase.Client;
using ExpensesManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExpensesManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Client _supabaseClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(Client supabaseClient, ILogger<AuthController> logger)
        {
            _supabaseClient = supabaseClient;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register(RegisterDto registerDto)
        {
            try
            {
                var userMetadata = new Dictionary<string, object>
                {
                    { "username", registerDto.Username }
                };

                var response = await _supabaseClient.Auth.SignUp(registerDto.Email, registerDto.Password, new SignUpOptions
                {
                    Data = userMetadata
                });

                var createdUser = response?.User;

                //crea il ruolo dell'utente
                await _supabaseClient.From<UserRole>().Insert(new UserRole
                {
                    UserId = createdUser.Id.ToString(),
                    Role = "User"
                });

                if (createdUser == null)
                {
                    throw new Exception("User creation failed.");
                }

                // prende tutte le cateogrie globali
                var globalCategoriesResponse = await _supabaseClient.From<GlobalCategory>().Get();
                var globalCategories = globalCategoriesResponse.Models;

                // copia le categorie globali per l'utente appena creato
                foreach (var globalCategory in globalCategories)
                {
                    var userCategory = new UserCategory
                    {
                        Name = globalCategory.Name,
                        Type = globalCategory.Type,
                        UserId = createdUser.Id.ToString()
                    };

                    await _supabaseClient.From<UserCategory>().Insert(userCategory);
                }

                var login = await _supabaseClient.Auth.SignIn(registerDto.Email, registerDto.Password);
				// query per ottenere il ruolo dell'utente
				var roleResponse = await _supabaseClient
                    .From<UserRole>()
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, login.User.Id)
                    .Get();

				if (roleResponse.Models.Any())
				{
					_logger.LogInformation("Ruolo trovato per l'utente: " + roleResponse.Models.First().Role);
				}
				else
				{
					_logger.LogWarning("Nessun ruolo trovato per l'utente con ID: " + login.User.Id);
				}

				var userRole = roleResponse.Models.FirstOrDefault()?.Role;

				var loginResponse = new LoginResponse
				{
					UserId = login.User.Id.ToString(),
					Username = registerDto.Username,
					AccessToken = login.AccessToken,
					RefreshToken = login.RefreshToken,
					Role = userRole
				};


				return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
 
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginDto loginDto)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignIn(loginDto.Email, loginDto.Password);

                var userMetadata = response?.User?.UserMetadata as IDictionary<string, object>;
                string username = userMetadata != null && userMetadata.ContainsKey("username") ? userMetadata["username"].ToString() : null;

				var roleResponse = await _supabaseClient
					.From<UserRole>()
					.Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, response.User.Id)
					.Get();
				var userRole = roleResponse.Models.FirstOrDefault()?.Role;

				var loginResponse = new LoginResponse
                {
                    UserId = response.User.Id.ToString(),
                    Username = username,
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                    Role = userRole
                };
                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("totalUsers")]
        public async Task<ActionResult<TotalUsers>> GetTotalUsers()
        {
            try
            {
                var allUsers = await _supabaseClient.From<UserRole>().Get();

                var total = allUsers.Models.Count;
                var totalAdmin = allUsers.Models.Count(user => user.Role == "Admin");
                var totalUser = allUsers.Models.Count(user => user.Role == "User");

                return Ok(new TotalUsers
                {
                    Total = total,
                    TotalAdmin = totalAdmin,
                    TotalUser = totalUser
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

		[Authorize]
        [HttpGet("getUserIds")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserIds()
		{
			try
			{
				var allUsers = await _supabaseClient
                    .From<UserRole>()
                    .Filter("role", Supabase.Postgrest.Constants.Operator.Equals, "User")
                    .Get();

				var userIds = allUsers.Models.Select(user => user.UserId);

				return Ok(userIds);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}

    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Role { get; set; }
    }

    public class TotalUsers
    {
        public int Total { get; set; }
        public int TotalUser { get; set; }
        public int TotalAdmin { get; set; }
    }
}