using EmailPOC.Events;
using EmailPOC.Interfaces;
using MediatR;
using Umbraco.Cms.Core.Services;

namespace EmailPOC.Handlers
{
    public class SendNewsletterHandler : INotificationHandler<SendNewsletterEvent>
    {

        private readonly IEmailHelper _emailHelper;
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;
        private readonly ILogger<SendNewsletterHandler> _logger;

        public SendNewsletterHandler(IEmailHelper emailHelper, IMemberService memberService, IMemberGroupService memberGroupService, ILogger<SendNewsletterHandler> logger)
        {
            _emailHelper = emailHelper;
            _memberService = memberService;
            _memberGroupService = memberGroupService;
            _logger = logger;
        }
        public async Task Handle(SendNewsletterEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                // Fetch subscribers
                var newsletterGroupKey = new Guid("0a4a4c5c-ded9-4193-a39e-885dd78839dd");
                var newsletterGroup = _memberGroupService.GetById(newsletterGroupKey);

                if (newsletterGroup == null)
                {
                    _logger.LogError("Newsletter group not found.");
                    return;
                }

                var subscribers = _memberService.GetMembersInRole(newsletterGroup.Name);

                // Send emails
                foreach (var subscriber in subscribers)
                {
                    string email = subscriber.Email;
                    bool success = await _emailHelper.SendEmail(email, notification.Subject, notification.Body);
                    if (!success)
                    {
                        _logger.LogError($"Failed to send email to {email}.");
                    }
                }

                _logger.LogInformation("Newsletter emails sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending newsletters.");
            }
        }
    }
}

