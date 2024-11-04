
namespace FunctionAppServiceBusRetryPolicy
{
    public record MessagingOptions
    {
        public const string Messaging = "Messaging";
        public string? RetryCountProperty { get; set; }
        public string? SequenceProperty { get; set; }
        public int RetryCount { get; set; }
    }
}
