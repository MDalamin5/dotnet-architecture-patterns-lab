using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TEcommerceWebApi.Events;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendOrderConfirmationEmailAsync(OrderPlacedEvent orderEvent)
        {
            _logger.LogInformation("📨 [EMAIL DISPATCH] Preparing confirmation email for Order #{OrderId}...", orderEvent.OrderId);

            // Simulate heavy background work (e.g. Generating PDF invoice + SMTP handshake)
            // In a real app, you would use SmtpClient, SendGrid, or AWS SES here!
            await Task.Delay(2500); // 2.5 seconds simulated latency

            _logger.LogInformation(
                "✅ [EMAIL SENT] Confirmation sent to {Email} ({Name}) for ${Amount} ({Items} items) on Store #{TenantId}!",
                orderEvent.CustomerEmail,
                orderEvent.CustomerName,
                orderEvent.TotalAmount,
                orderEvent.TotalItems,
                orderEvent.TenantId
            );
        }
    }
}