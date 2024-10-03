using EmailPOC.BackgroundJobs;
using Serilog;
using Serilog.Formatting.Compact;


namespace EmailPOC.Extensions
{
    public static class LoggerExtensions
    {
        public static Serilog.ILogger CreateNewsletterLogger(this LoggerConfiguration config, string? outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                outputPath = "\\Logs"; //Default File Path

            string file = outputPath + $"\\{DateTime.Now:yyyy-MM-dd} - {nameof(NewsletterMailSchedulerBackgroundJob)}.json";

            return config
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [" + "NewsLetter Job" + " {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    file,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger()
                .ForContext<NewsletterMailSchedulerBackgroundJob>();
        }

    }
}
