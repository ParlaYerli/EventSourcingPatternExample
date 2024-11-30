using EventSourcing.API.Context;
using EventSourcing.API.Context.DTO;
using EventSourcing.API.EventStores;
using EventSourcing.Common;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.API.Services
{
    public class OrderReadModelEventStore : BackgroundService
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly ILogger<OrderReadModelEventStore> _logger;
        private readonly IServiceProvider _serviceProvider;

        public OrderReadModelEventStore(IEventStoreConnection eventStoreConnection, ILogger<OrderReadModelEventStore> logger, IServiceProvider serviceProvider)
        {
            _eventStoreConnection = eventStoreConnection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderReadModelEventStore is starting...");
            await _eventStoreConnection.ConnectToPersistentSubscriptionAsync(
                OrderStream.StreamName,
                OrderStream.GroupName,
                EventAppeared,
                (_, reason, exception) =>
                    _logger.LogError($"Subscription dropped. Reason: {reason}, Exception: {exception?.Message}"),
                autoAck: false);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderReadModelEventStore background task running.");
            await Task.CompletedTask; 
        }

        private async Task EventAppeared(EventStorePersistentSubscriptionBase arg1, ResolvedEvent arg2)
        {
            using var scope = _serviceProvider.CreateScope(); 
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var type = Type.GetType($"{Encoding.UTF8.GetString(arg2.Event.Metadata)}, EventSourcing.Common");
            _logger.LogInformation($"Processing Message: {type}");

            var eventData = Encoding.UTF8.GetString(arg2.Event.Data);
            var @event = JsonSerializer.Deserialize(eventData, type);

            Order order = null;

            switch (@event)
            {
                case OrderCreatedEvent createdEvent:
                    order = new Order
                    {
                        Name = createdEvent.Name,
                        Id = createdEvent.Id,
                        Price = createdEvent.Price,
                        Stock = createdEvent.Stock,
                        UserId = createdEvent.UserId
                    };
                    context.Orders.Add(order);
                    break;

                case OrderNameUpdatedEvent nameChangedEvent:
                    order = await context.Orders.FindAsync(nameChangedEvent.Id);
                    if (order != null)
                        order.Name = nameChangedEvent.ChangedOrderName;
                    break;

                case OrderPriceUpdatedEvent priceChangedEvent:
                    order = await context.Orders.FindAsync(priceChangedEvent.Id);
                    if (order != null)
                        order.Price = priceChangedEvent.ChangedOrderPrice;
                    break;

                case OrderDeletedEvent deletedEvent:
                    order = await context.Orders.FindAsync(deletedEvent.Id);
                    if (order != null)
                        context.Orders.Remove(order);
                    break;

                default:
                    _logger.LogWarning($"Unrecognized event type: {type}");
                    break;
            }

            if (order != null)
            {
                await context.SaveChangesAsync();
                arg1.Acknowledge(arg2.Event.EventId);
            }
            else
            {
                _logger.LogWarning($"Event {arg2.Event.EventId} not processed.");
            }
        }
    }
}