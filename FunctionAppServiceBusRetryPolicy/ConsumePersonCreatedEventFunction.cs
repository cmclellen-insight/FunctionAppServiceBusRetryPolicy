using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunctionAppServiceBusRetryPolicy
{
    public class ConsumePersonCreatedEventFunction(
        ILogger<ConsumePersonCreatedEventFunction> logger, 
        IOptions<MessagingOptions> messagingOptions,
        ServiceBusClient serviceBusClient)
    {
        [Function(nameof(ConsumePersonCreatedEvent))]
        public async Task ConsumePersonCreatedEvent([ServiceBusTrigger("matching-runautomatch", Connection = "ServiceBusConnection", AutoCompleteMessages = false)] ServiceBusReceivedMessage receivedMessage,
            ServiceBusMessageActions messageActions)
        {
            await Task.CompletedTask;
            try {
                var personCreatedEvent = receivedMessage.Body.ToObjectFromJson<PersonCreatedEvent>();
                logger.LogInformation("C# HTTP trigger function processed a request. " + personCreatedEvent.Name);
                throw new Exception();
                //return new OkObjectResult("Welcome to Azure Functions!");
                //await messageActions.CompleteMessageAsync(receivedMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var sender = serviceBusClient.CreateSender("matching-runautomatch");
                await receivedMessage
                     .ExponentialRetry(ex, messageActions, sender, messagingOptions.Value, logger)
                     .ConfigureAwait(false);
            }
        }
    }
}
