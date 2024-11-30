using EventSourcing.API.EventStores;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using EventSourcing.API.Handlers.Commands;

namespace EventSourcing.API.Handlers.CommandHandlers
{
    public class UpdateOrderPriceCommandHandler : IRequestHandler<UpdateOrderPriceCommand>
    {
        private readonly OrderStream _orderStream;
        public UpdateOrderPriceCommandHandler(OrderStream orderStream)
        {
            _orderStream = orderStream;
        }
        public async Task<Unit> Handle(UpdateOrderPriceCommand request, CancellationToken cancellationToken)
        {
            _orderStream.PriceChanged(request.UpdateOrderPriceModel);
            await _orderStream.SaveAsync();

            return Unit.Value;
        }
    }
}
