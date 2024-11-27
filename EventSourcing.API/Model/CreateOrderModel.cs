namespace EventSourcing.API.Model
{
    public class CreateOrderModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}
