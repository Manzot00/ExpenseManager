namespace ExpenseManagement.Services
{
	// Crea spese o introti in base ai pagamenti regolari
	public class RegularPaymentService : IRegularPaymentService
	{
		private readonly IExpenseManagementAPIClient _apiClient;
		private readonly ILogger<RegularPaymentService> _logger;
		private readonly AuthenticationService _authService;

		public RegularPaymentService(IExpenseManagementAPIClient apiClient, ILogger<RegularPaymentService> logger, AuthenticationService authService)
		{
			_apiClient = apiClient;
			_logger = logger;
			_authService = authService;
		}

		public async Task ProcessRegularPayments()
		{
			try
			{
				// Ottieni il token di accesso e impostalo nel client API
				var accessToken = await _authService.GetAccessTokenAsync();
				_apiClient.SetAccessToken(accessToken);

				_logger.LogInformation("Processing regular payments.");
				var regularPayments = await _apiClient.GetAllUsersRPsAsync();
				var today = DateTime.Now.Date;
				today.AddHours(3); // Imposta l'ora a 3:00 AM per evitare problemi con i fusi orari

				foreach (var payment in regularPayments)
				{
					if (payment.RecurrenceDay == today.Day)
					{
						_logger.LogInformation($"Processing payment: {payment.Name} for user {payment.UserId}");
						if (payment.Type == "Expense")
						{
							await _apiClient.CreateExpenseAsync(new ExpenseDto
							{
								Name = payment.Name,
								Amount = payment.Amount,
								Date = DateTime.Now.Date.AddHours(3),
								CategoryId = payment.CategoryId,
								Note = payment.Note,
								UserId = payment.UserId
							});
							_logger.LogInformation($"Expense created: {payment.Name} for user {payment.UserId}");
						}
						else if (payment.Type == "Income")
						{
							await _apiClient.CreateIncomeAsync(new IncomeDto
							{
								Name = payment.Name,
								Amount = payment.Amount,
								Date = DateTime.Now.Date.AddHours(3),
								CategoryId = payment.CategoryId,
								Note = payment.Note,
								UserId = payment.UserId
							});
							_logger.LogInformation($"Income created: {payment.Name} for user {payment.UserId}");
						}
					}
				}

				_logger.LogInformation("Finished processing regular payments.");
				await _apiClient.LogoutAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error processing regular payments: {ex.Message}");
			}
		}
	}
}
