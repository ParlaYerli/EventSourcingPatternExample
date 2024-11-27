using System;

namespace EventSourcing.Common
{
    public class OrderNameUpdatedEvent : IEvent
    {
        public Guid Id { get; set; }
        public string ChangedOrderName { get; set; }
    }
}
