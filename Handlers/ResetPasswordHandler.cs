using EmailPOC.Events;
using EmailPOC.Interfaces;
using MediatR;

namespace EmailPOC.Handlers
{
    public class ResetPasswordHandler : INotificationHandler<ResetPasswordEvent>
    {
        private readonly IEmailHelper _emailHelper;

        public ResetPasswordHandler(IEmailHelper emailHelper)
        {
            _emailHelper = emailHelper;
        }

        public async Task Handle(ResetPasswordEvent notification, CancellationToken cancellationToken)
        {
            // Email content and sending logic
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", "ResetPasswordTemplate.html");
            string emailTemplate = await File.ReadAllTextAsync(templatePath);
            string emailBody = emailTemplate;
            string subject = "Password Reset Request";


            await _emailHelper.SendEmail(notification.Email, subject, emailBody);
        }
    }

}
