using EventSourcing.API.EventStores;
using EventSourcing.API.Handlers.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.API.Handlers.CommandHandlers
{
    public class UpdateOrderNameCommandHandler : IRequestHandler<UpdateOrderNameCommand>
    {
        private readonly OrderStream _orderStream;
        public UpdateOrderNameCommandHandler(OrderStream orderStream)
        {
            _orderStream = orderStream;
        }
        public async Task<Unit> Handle(UpdateOrderNameCommand request, CancellationToken cancellationToken)
        {
            _orderStream.NameUpdated(request.UpdateOrderNameModel);
            await _orderStream.SaveAsync();

            return Unit.Value;
        }
    }
}
