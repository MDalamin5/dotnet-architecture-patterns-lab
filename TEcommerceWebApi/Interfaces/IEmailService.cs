using System.Threading.Tasks;
using TEcommerceWebApi.Events;

namespace TEcommerceWebApi.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmailAsync(OrderPlacedEvent orderEvent);
    }
}