namespace EmailPOC.Settings
{
    public class NewsletterSettings
    {
        public string LoggerPath { get; set; } = string.Empty;
        public int RunsEveryInSeconds { get; set; }
        public int SheduledDateThresholdInMinutes { get; set; }
        public bool ShouldBypassDateValidation { get; set; }
        public int MaximumNumberOfRetries { get; set; }
        public int RetryThrottleInSeconds { get; set; }
    }
}
