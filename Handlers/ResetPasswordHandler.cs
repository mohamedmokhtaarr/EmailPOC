using EmailPOC.Events;
using EmailPOC.Helpers;
using EmailPOC.Interfaces;
using MediatR;

namespace EmailPOC.Handlers
{
    public class ResetPasswordHandler :  BaseEmailHandler , INotificationHandler<ResetPasswordEvent>
    {

        public ResetPasswordHandler(IEmailHelper emailHelper) : base(emailHelper)
        {
        }

        public async Task Handle(ResetPasswordEvent notification, CancellationToken cancellationToken)
        {
            // Email content and sending logic
            string emailTemplate = await LoadEmailTemplateAsync("ResetPasswordTemplate");
            
            var placeholders = new Dictionary<string, string>
            {
                { "Username", notification.Email },  // Use email as username or fetch username
                { "ResetLink", "https://www.google.com" }  // Generate reset link
            };
            string emailBody = ReplacePlaceholders(emailTemplate, placeholders);
            string subject = "Password Reset Request";


            await SendEmailAsync(notification.Email, subject, emailBody);
        }
    }

}
