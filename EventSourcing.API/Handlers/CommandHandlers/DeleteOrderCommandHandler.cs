using EventSourcing.API.EventStores;
using EventSourcing.API.Handlers.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.API.Handlers.CommandHandlers
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
    {
        private readonly OrderStream _orderStream;
        public DeleteOrderCommandHandler(OrderStream orderStream)
        {
            _orderStream = orderStream;
        }
        public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            _orderStream.Removed(request.Id);
            await _orderStream.SaveAsync();

            return Unit.Value;
        }
    }
}
