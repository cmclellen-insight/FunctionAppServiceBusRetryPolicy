using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionAppServiceBusRetryPolicy
{
    public static class ServiceBusExtensions
    {
        private static TimeSpan CalculateExponentialBackoff(int deliveryCount)
        {
            var interval = 1;
            var exponentialRate = 2;
            var maxDuration = TimeSpan.FromMinutes(2);
            var duration = TimeSpan.FromSeconds(interval * Math.Pow(exponentialRate, deliveryCount));
            return duration > maxDuration ? maxDuration : duration;
        }

        public static async Task ExponentialRetry(
        [NotNull] this ServiceBusReceivedMessage receivedMessage,
        [NotNull] Exception ex,
        [NotNull] ServiceBusMessageActions messageActions,
        [NotNull] ServiceBusSender sender,
        [NotNull] MessagingOptions messagingOptions,
        [NotNull] ILogger log)
        {
            // If the message doesn't have a retry-count, set as 0
            var retryMessage = new ServiceBusMessage(receivedMessage);
            if (!receivedMessage.ApplicationProperties.ContainsKey(messagingOptions.RetryCountProperty))
            {
                retryMessage.ApplicationProperties.Add(messagingOptions.RetryCountProperty, 0);
                retryMessage.ApplicationProperties.Add(messagingOptions.SequenceProperty, receivedMessage.SequenceNumber);
            }

            var retryAttempt = (int)retryMessage.ApplicationProperties[messagingOptions.RetryCountProperty];
            if (retryAttempt < messagingOptions.RetryCount)
            {
                retryAttempt += 1;
                var interval = CalculateExponentialBackoff(retryAttempt);
                var scheduledTime = DateTimeOffset.Now.Add(interval);

                retryMessage.ApplicationProperties[messagingOptions.RetryCountProperty] = retryAttempt;
                await sender.ScheduleMessageAsync(retryMessage, scheduledTime).ConfigureAwait(false);
                await messageActions.CompleteMessageAsync(receivedMessage).ConfigureAwait(false);

                log.LogWarning("Scheduling message retry {RetryCount} to wait {Interval} seconds and arrive at {ScheduledTime}", retryAttempt, interval.TotalSeconds, scheduledTime.UtcDateTime);
            }
            else
            {
                var seqNumber = receivedMessage.ApplicationProperties[messagingOptions.SequenceProperty];
                log.LogCritical("Exhausted all retries for message sequence # {SequenceNumber}", seqNumber);
                await messageActions.DeadLetterMessageAsync(receivedMessage, null, ex.Message, ex.ToString()).ConfigureAwait(false);
            }
        }
    }
}
