using EventSourcing.API.Context.DTO;
using EventSourcing.API.Context;
using EventSourcing.Common;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.API.EventStores
{
    public abstract class AbstractStream
    {
        protected readonly LinkedList<IEvent> Events = new LinkedList<IEvent>();

        private readonly string _streamName;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IServiceProvider _serviceProvider;

        protected AbstractStream(string streamName, IEventStoreConnection eventStoreConnection, IServiceProvider serviceProvider)
        {
            _streamName = streamName;
            _eventStoreConnection = eventStoreConnection;
            _serviceProvider = serviceProvider;
        }

        public async Task SaveAsync()
        {
            try
            {
                var newEvents = Events.Select(x => new EventData(
                    Guid.NewGuid(),
                    x.GetType().Name,
                    true,
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(x, inputType: x.GetType())),
                    Encoding.UTF8.GetBytes(x.GetType().FullName)
                )).ToList();

                await _eventStoreConnection.AppendToStreamAsync(_streamName, ExpectedVersion.Any, newEvents);

                await ProcessEventsToDatabase();

                Events.Clear();
            }
            catch (Exception ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        private async Task ProcessEventsToDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var @event in Events)
            {
                switch (@event)
                {
                    case OrderCreatedEvent createdEvent:
                        context.Orders.Add(new Order
                        {
                            Id = createdEvent.Id,
                            Name = createdEvent.Name,
                            Price = createdEvent.Price,
                            Stock = createdEvent.Stock,
                            UserId = createdEvent.UserId
                        });
                        break;

                    case OrderNameUpdatedEvent nameUpdatedEvent:
                        var orderNameUpdate = context.Orders.Find(nameUpdatedEvent.Id);
                        if (orderNameUpdate != null)
                        {
                            orderNameUpdate.Name = nameUpdatedEvent.ChangedOrderName;
                        }
                        break;

                    case OrderPriceUpdatedEvent priceUpdatedEvent:
                        var orderPriceUpdate = context.Orders.Find(priceUpdatedEvent.Id);
                        if (orderPriceUpdate != null)
                        {
                            orderPriceUpdate.Price = priceUpdatedEvent.ChangedOrderPrice;
                        }
                        break;

                    case OrderDeletedEvent deletedEvent:
                        var orderDelete = context.Orders.Find(deletedEvent.Id);
                        if (orderDelete != null)
                        {
                            context.Orders.Remove(orderDelete);
                            await context.SaveChangesAsync();
                        }
                        break;
                }
            }

            await context.SaveChangesAsync();
        }
    }

}



