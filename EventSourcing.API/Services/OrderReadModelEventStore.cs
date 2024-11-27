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

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _eventStoreConnection.ConnectToPersistentSubscriptionAsync(OrderStream.StreamName, OrderStream.GroupName, EventAppeared, autoAck: false);

        }

        private async Task EventAppeared(EventStorePersistentSubscriptionBase arg1, ResolvedEvent arg2)
        {
            var type = Type.GetType($"{Encoding.UTF8.GetString(arg2.Event.Metadata)}, EventSourcing.Common");
            _logger.LogInformation($"The Message processing... : {type.ToString()}");
            var eventData = Encoding.UTF8.GetString(arg2.Event.Data);

            var @event = JsonSerializer.Deserialize(eventData, type);

            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Order order = null;

            switch (@event)
            {
                case OrderCreatedEvent productCreatedEvent:

                    order = new Order()
                    {
                        Name = productCreatedEvent.Name,
                        Id = productCreatedEvent.Id,
                        Price = productCreatedEvent.Price,
                        Stock = productCreatedEvent.Stock,
                        UserId = productCreatedEvent.UserId
                    };
                    context.Orders.Add(order);
                    break;

                case OrderNameUpdatedEvent orderNameChangedEvent:

                    order = context.Orders.Find(orderNameChangedEvent.Id);
                    if (order != null)
                    {
                        order.Name = orderNameChangedEvent.ChangedOrderName;
                    }
                    break;

                case OrderPriceUpdatedEvent orderPriceChangedEvent:
                    order = context.Orders.Find(orderPriceChangedEvent.Id);
                    if (order != null)
                    {
                        order.Price = orderPriceChangedEvent.ChangedOrderPrice;
                    }
                    break;

                case OrderDeletedEvent orderDeletedEvent:
                    order = context.Orders.Find(orderDeletedEvent.Id);
                    if (order != null)
                    {
                        context.Orders.Remove(order);
                    }
                    break;
            }

            await context.SaveChangesAsync();

            arg1.Acknowledge(arg2.Event.EventId);
        }
    }
}