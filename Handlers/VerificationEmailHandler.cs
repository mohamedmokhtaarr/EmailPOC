﻿using EmailPOC.Events;
using EmailPOC.Helpers;
using EmailPOC.Interfaces;
using MediatR;

namespace EmailPOC.Handlers
{
    public class VerificationEmailHandler : BaseEmailHandler, INotificationHandler<VerificationEmailEvent>
    {
        public VerificationEmailHandler(IEmailHelper emailHelper) : base(emailHelper)
        {
        }

        public async Task Handle(VerificationEmailEvent notification, CancellationToken cancellationToken)
        {
            // Email content and sending logic
            string emailTemplate = await LoadEmailTemplateAsync("VerificationEmailTemplate");

            var placeholders = new Dictionary<string, string>
            {
                { "Username", notification.Email },  // Use email as username or fetch username
                { "VerificationLink", "https://www.google.com" }  // Generate reset link
            };
            string emailBody = ReplacePlaceholders(emailTemplate, placeholders);
            string subject = "Verification Email";


            await SendEmailAsync(notification.Email, subject, emailBody);
        }
    }
}
