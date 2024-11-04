using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunctionAppServiceBusRetryPolicy
{  
    public class ServiceBusRetryHandlingMiddleware(
        ILogger<ServiceBusRetryHandlingMiddleware> logger,
        IOptions<MessagingOptions> messagingOptions,
        ServiceBusClient serviceBusClient) : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // TODO
        }
    }
}
