using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TEcommerceWebApi.Events;

namespace TEcommerceWebApi.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        // Producer: Pushes order event into the queue
        ValueTask QueueOrderPlacedEventAsync(OrderPlacedEvent orderEvent);

        // Consumer: Reads order events as they arrive
        IAsyncEnumerable<OrderPlacedEvent> ReadAllAsync(CancellationToken cancellationToken);
    }
}