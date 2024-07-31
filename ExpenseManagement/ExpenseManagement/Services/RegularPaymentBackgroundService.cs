namespace ExpenseManagement.Services
{
	public class RegularPaymentBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<RegularPaymentBackgroundService> _logger;

		public RegularPaymentBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<RegularPaymentBackgroundService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using (var scope = _serviceScopeFactory.CreateScope())
				{
					var regularPaymentService = scope.ServiceProvider.GetRequiredService<IRegularPaymentService>();

					try
					{
						await regularPaymentService.ProcessRegularPayments();
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error processing regular payments");
					}
				}

				// Attende un giorno prima di eseguire di nuovo il controllo
				await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
			}
		}
	}
}
