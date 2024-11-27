using EventSourcing.API.Model;
using MediatR;
using System.Collections.Generic;

namespace EventSourcing.API.Handlers.Queries
{
    public class GetOrderAllListByUserId : IRequest<List<OrderModel>>
    {
        public int UserId { get; set; }
    }
}
