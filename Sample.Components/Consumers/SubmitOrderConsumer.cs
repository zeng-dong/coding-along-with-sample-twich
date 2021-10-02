using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.Log(LogLevel.Debug, "SubmitOrderConsumer: {CustomerNumber}", context.Message.CustomerNumber);

            if (context.Message.CustomerNumber.Contains("TEST"))
            {
                //if (context.ResponseAddress != null)  // check need response by checking address or requestid
                if (context.RequestId != null)
                {
                    await context.RespondAsync<OrderSubmissionRejected>(new
                    {
                        InVar.Timestamp,
                        context.Message.OrderId,
                        context.Message.CustomerNumber,
                        Reason = $"Test Customer cannot submit orders: {context.Message.CustomerNumber}"
                    });

                    return;
                }
            }

            await context.Publish<OrderSubmitted>(new
            {
                context.Message.OrderId,
                context.Message.Timestamp,
                context.Message.CustomerNumber
            });


            if (context.ResponseAddress != null)
            {
                await context.RespondAsync<OrderSubmissionAccepted>(new
                {
                    InVar.Timestamp
,
                    context.Message.OrderId,
                    context.Message.CustomerNumber
                });
            }
        }
    }
}
