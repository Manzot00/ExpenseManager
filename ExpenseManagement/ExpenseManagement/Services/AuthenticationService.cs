using Microsoft.Extensions.Caching.Memory;

namespace ExpenseManagement.Services
{
	//Utilizzato per accedere come admin quando si aggiornano i pagamenti regolari
	public class AuthenticationService
	{
		private readonly IExpenseManagementAPIClient _apiClient;
		private readonly IMemoryCache _cache;
		private readonly ILogger<AuthenticationService> _logger;
		private readonly string _email;
		private readonly string _password;

		public AuthenticationService(IExpenseManagementAPIClient apiClient, IMemoryCache cache, ILogger<AuthenticationService> logger, IConfiguration configuration)
		{
			_apiClient = apiClient;
			_cache = cache;
			_logger = logger;
			_email = configuration["Admin:Email"];
			_password = configuration["Admin:Password"];
		}

		public async Task<string> GetAccessTokenAsync()
		{
			if (!_cache.TryGetValue("AccessToken", out string accessToken))
			{
				var response = await _apiClient.LoginAsync(new LoginDto { Email = _email, Password = _password });
				if (response != null && !string.IsNullOrEmpty(response.AccessToken))
				{
					accessToken = response.AccessToken;
					_cache.Set("AccessToken", accessToken, TimeSpan.FromMinutes(30)); // Imposta un timeout appropriato
				}
				else
				{
					_logger.LogError("Failed to log in and retrieve access token.");
				}
			}
			return accessToken;
		}
	}
}
