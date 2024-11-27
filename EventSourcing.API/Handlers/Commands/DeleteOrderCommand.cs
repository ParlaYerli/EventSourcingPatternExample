using MediatR;
using System;

namespace EventSourcing.API.Handlers.Commands
{
    public class DeleteOrderCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
