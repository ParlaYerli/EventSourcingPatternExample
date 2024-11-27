using System;

namespace EventSourcing.Common
{
    public class OrderDeletedEvent : IEvent
    {
        public Guid Id { get; set; } 
    }
}
