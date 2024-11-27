using EventSourcing.API.Handlers.Commands;
using EventSourcing.API.Handlers.Queries;
using EventSourcing.API.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EventSourcing.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAllListByUserId(int userId)
        {
            return Ok(await _mediator.Send(new GetOrderAllListByUserId() { UserId = userId }));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderModel model)
        {
            await _mediator.Send(new CreateOrderCommand() { CreateOrderModel = model });
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateName(UpdateOrderNameModel model)
        {
            await _mediator.Send(new UpdateOrderNameCommand() { UpdateOrderNameModel = model });
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePrice(UpdateOrderPriceModel model)
        {
            await _mediator.Send(new UpdateOrderPriceCommand() { UpdateOrderPriceModel = model });
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            await _mediator.Send(new DeleteOrderCommand() { Id = id });
            return NoContent();
        }

    }
}
