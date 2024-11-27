using System;

namespace EventSourcing.API.Model
{
    public class UpdateOrderPriceModel
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; } 
    }
}
