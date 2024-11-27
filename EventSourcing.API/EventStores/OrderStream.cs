using EventSourcing.API.Model;
using EventSourcing.Common;
using EventStore.ClientAPI;
using System;

namespace EventSourcing.API.EventStores
{
    public class OrderStream : AbstractStream
    {
        public static string StreamName => "Order";
        public static string GroupName => "replay";
        public OrderStream(IEventStoreConnection connection) : base(StreamName, connection)
        {
        }

        public void Created(CreateOrderModel model)
        {
            Events.AddLast(new OrderCreatedEvent 
            { 
                Id = Guid.NewGuid(),
                Name = model.Name,
                Price = model.Price,
                Stock = model.Stock,
                UserId = model.UserId,
            });
        }
        public void NameUpdated(UpdateOrderNameModel model)
        {
            Events.AddLast(new OrderNameUpdatedEvent
            {
                Id = model.Id,
                ChangedOrderName = model.Name,
            });
        }
        public void PriceUpdated(UpdateOrderPriceModel model)
        {
            Events.AddLast(new OrderPriceUpdatedEvent
            {
                Id = model.Id,
                ChangedOrderPrice = model.Price,
            });
        }
        public void Removed(Guid id)
        {
            Events.AddLast(new OrderDeletedEvent { Id = id });
        }
    }
}
