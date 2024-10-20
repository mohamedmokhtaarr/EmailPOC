using EmailPOC.Interfaces;

namespace EmailPOC.Helpers
{
    public abstract class BaseEmailHandler
    {
        private readonly IEmailHelper _emailHelper;

        protected BaseEmailHandler(IEmailHelper emailHelper)
        {
            _emailHelper = emailHelper;
        }

        // Method to load email template
        protected async Task<string> LoadEmailTemplateAsync(string templateName)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", $"{templateName}.html");
            return await File.ReadAllTextAsync(templatePath);
        }

        // Method to replace placeholders in the template
        protected string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
        {
            foreach (var placeholder in placeholders)
            {
                // Replace each placeholder in the template
                template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            return template;
        }

        // Method to send an email
        protected async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            await _emailHelper.SendEmail(recipientEmail, subject, body);
        }
    }

}
