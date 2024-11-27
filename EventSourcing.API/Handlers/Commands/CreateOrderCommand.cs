using EventSourcing.API.Model;
using MediatR;

namespace EventSourcing.API.Handlers.Commands
{
    public class CreateOrderCommand : IRequest
    {
        public CreateOrderModel CreateOrderModel { get; set; }
    }
}
