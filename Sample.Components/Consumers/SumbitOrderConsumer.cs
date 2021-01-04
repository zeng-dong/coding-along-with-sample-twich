using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace Sample.Components.Consumers
{
    public class SumbitOrderConsumer : IConsumer<SubmitOrder>
    {
        readonly ILogger<SumbitOrderConsumer> _logger;

        public SumbitOrderConsumer(ILogger<SumbitOrderConsumer> logger)
        {
            _logger = logger;
        }
        
        public async  Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.Log(LogLevel.Debug, "SubmitOrderConsumer: {CustomerNumber}", context.Message.CustomerNumber);
            if (context.Message.CustomerNumber.Contains("TEST"))
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