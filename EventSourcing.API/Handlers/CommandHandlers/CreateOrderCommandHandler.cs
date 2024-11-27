using EventSourcing.API.EventStores;
using EventSourcing.API.Handlers.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.API.Handlers.CommandHandlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand>
    {
        private readonly OrderStream _orderStream;
        public CreateOrderCommandHandler(OrderStream orderStream)
        {
            _orderStream = orderStream;
        }
        public async Task<Unit> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _orderStream.Created(request.CreateOrderModel);
            await _orderStream.SaveAsync();
            return Unit.Value;
        }
    }
}
