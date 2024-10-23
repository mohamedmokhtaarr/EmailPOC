using EmailPOC.Interfaces;
using EmailPOC.DataAccess.Entities;
using EmailPOC.DataAccess;
using Microsoft.Extensions.Logging;

namespace EmailPOC.Helpers
{
    public abstract class BaseEmailHandler
    {
        private readonly IEmailHelper _emailHelper;
        private readonly NewsletterMailDbContext _dbContext;
        private readonly ILogger<BaseEmailHandler> _logger;

        protected BaseEmailHandler(IEmailHelper emailHelper, NewsletterMailDbContext dbContext, ILogger<BaseEmailHandler> logger)
        {
            _emailHelper = emailHelper;
            _dbContext = dbContext;
            _logger = logger;
        }

        protected async Task<string> LoadEmailTemplateAsync(string templateName, CancellationToken cancellationToken)
        {
            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", $"{templateName}.html");
                return await File.ReadAllTextAsync(templatePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading email template: {templateName}");
                throw; // Re-throw the exception to handle it at a higher level if needed
            }
        }

        protected string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
        {
            foreach (var placeholder in placeholders)
            {
                template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            return template;
        }

        protected async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken, int maxRetryAttempts = 3)
        {
            try
            {
                _logger.LogInformation($"Sending email to {recipientEmail} with subject {subject}.");

                bool isSent = await _emailHelper.SendEmail(recipientEmail, subject, body);

                // Log the email attempt
                await LogEmailAttempt(recipientEmail, subject, isSent, retryCount: 0, retryDate: null);

                if (isSent)
                {
                    _logger.LogInformation($"Email sent successfully to {recipientEmail}");
                }
                else
                {
                    _logger.LogWarning($"Failed to send email to {recipientEmail}, initiating retry mechanism.");

                    // Invoke the retry mechanism
                    await RetryEmailAsync(recipientEmail, subject, body, maxRetryAttempts, cancellationToken);
                }

                return isSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {recipientEmail}");
                throw;
            }
        }


        protected async Task LogEmailAttempt(string recipientEmail, string emailType, bool isSuccess, int retryCount, DateTime? retryDate = null)
        {
            try
            {
                var emailLog = new EmailLog
                {
                    Email = recipientEmail,
                    EmailType = emailType,
                    IsSent = isSuccess,
                    SentDate = DateTime.UtcNow, // Always log the time the email was initially sent or attempted
                    RetryCount = retryCount, // Track the number of retries
                    RetryDate = retryDate // Log the retry date if applicable
                };

                _dbContext.EmailLogs.Add(emailLog);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Logged email attempt for {recipientEmail} with status: {(isSuccess ? "Success" : "Failure")} and retry count: {retryCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging email attempt for {recipientEmail}");
            }
        }

        protected async Task RetryEmailAsync(string recipientEmail, string subject, string body, int maxRetryAttempts = 3, CancellationToken cancellationToken = default)
        {
            int retryCount = 0;
            bool isSent = false;

            while (retryCount < maxRetryAttempts && !isSent)
            {
                isSent = await SendEmailAsync(recipientEmail, subject, body, cancellationToken);
                retryCount++;

                if (!isSent)
                {
                    _logger.LogWarning($"Retrying email to {recipientEmail}. Attempt {retryCount}/{maxRetryAttempts}");

                    // Log the retry attempt with the retry date
                    await LogEmailAttempt(recipientEmail, subject, isSent, retryCount, DateTime.UtcNow);

                    // Exponential backoff between retry attempts
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);
                }
            }

            if (!isSent)
            {
                _logger.LogError($"Failed to send email to {recipientEmail} after {maxRetryAttempts} attempts.");
            }
        }


    }
}
