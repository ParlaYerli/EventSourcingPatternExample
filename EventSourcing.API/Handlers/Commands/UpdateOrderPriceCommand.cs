using EventSourcing.API.Model;
using MediatR;
using System;

namespace EventSourcing.API.Handlers.Commands
{
    public class UpdateOrderPriceCommand : IRequest
    {
        public UpdateOrderPriceModel UpdateOrderPriceModel { get; set; }
    }
}
