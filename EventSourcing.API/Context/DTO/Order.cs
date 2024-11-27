using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace EventSourcing.API.Context.DTO
{
    public class Order
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int UserId { get; set; }
    }
}
