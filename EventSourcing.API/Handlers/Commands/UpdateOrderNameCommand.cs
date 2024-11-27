using EventSourcing.API.Model;
using MediatR;

namespace EventSourcing.API.Handlers.Commands
{
    public class UpdateOrderNameCommand : IRequest
    {
        public UpdateOrderNameModel UpdateOrderNameModel { get; set; }
    }
}
