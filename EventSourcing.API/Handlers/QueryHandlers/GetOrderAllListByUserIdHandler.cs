using EventSourcing.API.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using EventSourcing.API.Model;
using EventSourcing.API.Handlers.Queries;
using MediatR;
using System.Linq;

namespace EventSourcing.API.Handlers.QueryHandlers
{
    public class GetOrderAllListByUserIdHandlerIRequestHandler : IRequestHandler<GetOrderAllListByUserId, List<OrderModel>>
    {
        private readonly AppDbContext _context;
        public GetOrderAllListByUserIdHandlerIRequestHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderModel>> Handle(GetOrderAllListByUserId request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders.Where(x => x.UserId == request.UserId).ToListAsync();

            return orders.Select(x => new OrderModel { Id = x.Id, Name = x.Name, Price = x.Price, Stock = x.Stock, UserId = x.UserId }).ToList();
        }
    }
}
