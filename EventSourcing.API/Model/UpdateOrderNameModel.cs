using System;

namespace EventSourcing.API.Model
{
    public class UpdateOrderNameModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
