using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class SumbitOrderConsumer : IConsumer<SubmitOrder>
    {
        readonly ILogger<SumbitOrderConsumer> _logger;

        public SumbitOrderConsumer(ILogger<SumbitOrderConsumer> logger)
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
