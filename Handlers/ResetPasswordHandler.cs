using EmailPOC.DataAccess;
using EmailPOC.Events;
using EmailPOC.Helpers;
using EmailPOC.Interfaces;
using MediatR;


namespace EmailPOC.Handlers
{
    public class ResetPasswordHandler :  BaseEmailHandler , INotificationHandler<ResetPasswordEvent>
    {
        public ResetPasswordHandler(IEmailHelper emailHelper,
            NewsletterMailDbContext dbContext,
            ILogger<BaseEmailHandler> logger) 
            : base(emailHelper, dbContext, logger)
        {
        }

        public async Task Handle(ResetPasswordEvent notification, CancellationToken cancellationToken)
        {
            // Email content and sending logic
            string emailTemplate = await LoadEmailTemplateAsync("ResetPasswordTemplate" , cancellationToken);
            
            var placeholders = new Dictionary<string, string>
            {
                { "Username", notification.Email },  
                { "ResetLink", "https://www.google.com" }
            };
            string emailBody = ReplacePlaceholders(emailTemplate, placeholders);
            string subject = "Password Reset Request";
           // await Task.Delay(1000000);
            await SendEmailAsync(notification.Email, subject, emailBody , cancellationToken);

        }
    }

}
