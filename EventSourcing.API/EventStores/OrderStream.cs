using EventSourcing.API.Model;
using EventSourcing.Common;
using EventStore.ClientAPI;
using System;

namespace EventSourcing.API.EventStores
{
    public class OrderStream : AbstractStream
    {
        public static string StreamName => "OrderStream3";
        public static string GroupName => "agroup3";

        public OrderStream(IEventStoreConnection eventStoreConnection) : base(StreamName, eventStoreConnection)
        {
        }

        public void Created(CreateOrderModel createProductDto)
        {
            Events.AddLast(new OrderCreatedEvent { Id = Guid.NewGuid(), Name = createProductDto.Name, Price = createProductDto.Price, Stock = createProductDto.Stock, UserId = createProductDto.UserId });
        }

        public void NameChanged(UpdateOrderNameModel changeProductNameDto)
        {
            Events.AddLast(new OrderNameUpdatedEvent { ChangedOrderName = changeProductNameDto.Name, Id = changeProductNameDto.Id });
        }

        public void PriceChanged(UpdateOrderPriceModel changeProductPriceDto)
        {
            Events.AddLast(new OrderPriceUpdatedEvent() { ChangedOrderPrice = changeProductPriceDto.Price, Id = changeProductPriceDto.Id });
        }

        public void Deleted(Guid id)
        {
            Events.AddLast(new OrderDeletedEvent { Id = id });
        }
    }
}
