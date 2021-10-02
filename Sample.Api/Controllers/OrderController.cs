using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System;
using System.Threading.Tasks;

namespace Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        readonly IRequestClient<SubmitOrder> _submitOrderRequestClient;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IRequestClient<CheckOrder> _checkOrderClient;

        public OrderController(ILogger<OrderController> logger,
            IRequestClient<SubmitOrder> submitOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider,
            IRequestClient<CheckOrder> checkOrderClient)
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _checkOrderClient = checkOrderClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var (status, notFound) = await _checkOrderClient.GetResponse<OrderStatus, OrderNotFound>(new
            {
                OrderId = id
            });

            if (status.IsCompletedSuccessfully)
            {
                var response = await status;
                return Ok(response.Message);    
            }
            else
            {
                var response = await notFound;
                return NotFound(response.Message);
            }
        }

        [HttpPost("request-and-wait-for-response")]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {
            var (accepted, rejected)
                = await _submitOrderRequestClient.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(new
                {
                    OrderId = id,
                    Timestamp = InVar.Timestamp,
                    CustomerNumber = customerNumber
                });

            if (accepted.IsCompletedSuccessfully)
            {
                var response = await accepted;
                return Ok(response.Message);
            }
            else
            {
                var response = await rejected;
                return BadRequest(response.Message);
            }
        }

        [HttpPut("send-and-forget")]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("exchange:submit-order"));

            await endpoint.Send<SubmitOrder>(new
            {
                OrderId = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            return Accepted();
        }
    }
}
