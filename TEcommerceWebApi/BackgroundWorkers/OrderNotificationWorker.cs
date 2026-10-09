using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.BackgroundWorkers
{
    public class OrderNotificationWorker : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderNotificationWorker> _logger;

        public OrderNotificationWorker(
            IBackgroundTaskQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderNotificationWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // This method starts automatically when the .NET app boots and runs on a background thread:
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 [WORKER STARTED] OrderNotificationWorker is listening for new orders...");

            try
            {
                // Streams items from the Channel as soon as they are pushed:
                await foreach (var orderEvent in _queue.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        _logger.LogInformation("📥 [WORKER RECEIVED] Processing Order #{OrderId} in background...", orderEvent.OrderId);

                        // 🔑 Bridge Singleton to Scoped lifetime:
                        using var scope = _scopeFactory.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        // Execute the email sending task:
                        await emailService.SendOrderConfirmationEmailAsync(orderEvent);
                    }
                    catch (Exception ex)
                    {
                        // Resilient design: A failed email should NEVER crash the background loop!
                        _logger.LogError(ex, "❌ [WORKER ERROR] Failed to send email for Order #{OrderId}", orderEvent.OrderId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown when app stops (e.g. during deployment)
                _logger.LogInformation("🛑 [WORKER STOPPED] OrderNotificationWorker is shutting down gracefully.");
            }
        }
    }
}