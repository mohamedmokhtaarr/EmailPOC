using EmailPOC.DataAccess;
using EmailPOC.Events;
using EmailPOC.Helpers;
using EmailPOC.Interfaces;
using MediatR;

namespace EmailPOC.Handlers
{
    public class VerificationEmailHandler : BaseEmailHandler, INotificationHandler<VerificationEmailEvent>
    {
        public VerificationEmailHandler(IEmailHelper emailHelper, NewsletterMailDbContext dbContext, ILogger<BaseEmailHandler> logger) : base(emailHelper, dbContext, logger)
        {
        }

        public async Task Handle(VerificationEmailEvent notification, CancellationToken cancellationToken)
        {
            // Email content and sending logic
            string emailTemplate = await LoadEmailTemplateAsync("VerificationEmailTemplate", cancellationToken);

            var placeholders = new Dictionary<string, string>
            {
                { "Username", notification.Email },  
                { "VerificationLink", "https://www.google.com" }  
            };
            string emailBody = ReplacePlaceholders(emailTemplate, placeholders);
            string subject = "Verification Email";


            await SendEmailAsync(notification.Email, subject, emailBody, cancellationToken);
        }
    }
}
