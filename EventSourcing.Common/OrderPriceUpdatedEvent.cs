using System;

namespace EventSourcing.Common
{
    public class OrderPriceUpdatedEvent : IEvent
    {
        public Guid Id { get; set; }
        public decimal ChangedOrderPrice { get; set; }
    }
}
