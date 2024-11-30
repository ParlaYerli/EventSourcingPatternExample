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

        public OrderReadModelEventStore(IEventStoreConnection eventStoreConnection,
                                        ILogger<OrderReadModelEventStore> logger,
                                        IServiceProvider serviceProvider)
        {
            _eventStoreConnection = eventStoreConnection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventStoreConnection.ConnectToPersistentSubscriptionAsync(OrderStream.StreamName,
                                                                             OrderStream.GroupName,
                                                                             EventAppeared,
                                                                             autoAck: false);
            _logger.LogInformation("Event Store Persistent Subscription connected.");
            await base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderReadModelEventStore service stopping...");
            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderReadModelEventStore background service running.");
            return Task.CompletedTask;
        }

        private async Task EventAppeared(EventStorePersistentSubscriptionBase subscription, ResolvedEvent resolvedEvent)
        {
            try
            {
                var metadataType = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);
                var eventType = Type.GetType($"{metadataType}, EventSourcing.Common");
                _logger.LogInformation($"Processing event: {eventType?.Name}");

                if (eventType == null)
                {
                    _logger.LogWarning("Event type not found in metadata. Skipping event.");
                    subscription.Fail(resolvedEvent, PersistentSubscriptionNakEventAction.Skip, "Unknown Event Type");
                    return;
                }

                var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                var @event = JsonSerializer.Deserialize(eventData, eventType);

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await ProcessEventAsync(@event, dbContext);

                await dbContext.SaveChangesAsync();

                subscription.Acknowledge(resolvedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event: {ex.Message}", ex);
                subscription.Fail(resolvedEvent, PersistentSubscriptionNakEventAction.Retry, ex.Message);
            }
        }

        private static async Task ProcessEventAsync(object @event, AppDbContext context)
        {
            Order order = null;

            switch (@event)
            {
                case OrderCreatedEvent orderCreatedEvent:
                    order = new Order
                    {
                        Id = orderCreatedEvent.Id,
                        Name = orderCreatedEvent.Name,
                        Price = orderCreatedEvent.Price,
                        Stock = orderCreatedEvent.Stock,
                        UserId = orderCreatedEvent.UserId
                    };
                    context.Orders.Add(order);
                    break;

                case OrderNameUpdatedEvent nameUpdatedEvent:
                    order = await context.Orders.FindAsync(nameUpdatedEvent.Id);
                    if (order != null)
                    {
                        order.Name = nameUpdatedEvent.ChangedOrderName;
                    }
                    break;

                case OrderPriceUpdatedEvent priceUpdatedEvent:
                    order = await context.Orders.FindAsync(priceUpdatedEvent.Id);
                    if (order != null)
                    {
                        order.Price = priceUpdatedEvent.ChangedOrderPrice;
                    }
                    break;

                case OrderDeletedEvent deletedEvent:
                    order = await context.Orders.FindAsync(deletedEvent.Id);
                    if (order != null)
                    {
                        context.Orders.Remove(order);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unhandled event type: {@event.GetType().Name}");
            }
        }
    }
}