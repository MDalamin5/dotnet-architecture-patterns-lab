using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TEcommerceWebApi.Events;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        // Channel acts as a thread-safe FIFO (First-In, First-Out) buffer
        private readonly Channel<OrderPlacedEvent> _channel;

        public BackgroundTaskQueue()
        {
            // Bounded channel: Holds up to 10,000 tasks in RAM to protect against memory overflow
            var options = new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.Wait, // If queue reaches 10,000, wait rather than dropping messages
                SingleReader = true,                   // Optimized for our background worker
                SingleWriter = false                   // Multiple web requests can write to the queue at the same time
            };

            _channel = Channel.CreateBounded<OrderPlacedEvent>(options);
        }

        // Producer method: Pushes an item into the queue
        public async ValueTask QueueOrderPlacedEventAsync(OrderPlacedEvent orderEvent)
        {
            await _channel.Writer.WriteAsync(orderEvent);
        }

        // Consumer method: Streams items to the background worker as they arrive
        public IAsyncEnumerable<OrderPlacedEvent> ReadAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}