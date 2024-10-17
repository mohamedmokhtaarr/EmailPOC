using EmailPOC.BackgroundJobs;
using EmailPOC.Events;
using EmailPOC.Interfaces;
using MediatR;
using Umbraco.Cms.Core.Services;

namespace EmailPOC.Handlers
{
    public class DailyEmailHandler : INotificationHandler<DailyEmailEvent>
    {
        private readonly IEmailHelper _emailHelper;
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;
        private readonly ILogger<NewsletterMailSchedulerBackgroundJob> _logger;
        public DailyEmailHandler(IEmailHelper emailHelper, IMemberService memberService, IMemberGroupService memberGroupService, ILogger<NewsletterMailSchedulerBackgroundJob> logger)
        {
            _emailHelper = emailHelper;
            _memberService = memberService;
            _memberGroupService = memberGroupService;
            _logger = logger;
        }

        public async Task Handle(DailyEmailEvent notification, CancellationToken cancellationToken)
        {
            var newsletterGroupKey = new Guid("0a4a4c5c-ded9-4193-a39e-885dd78839dd");
            var newsletterGroup = _memberGroupService.GetById(newsletterGroupKey);

            if (newsletterGroup == null)
            {
                _logger.LogError("Newsletter group not found.");
                return;
            }

            var subscribers = _memberService.GetMembersInRole(newsletterGroup.Name);

            foreach (var subscriber in subscribers)
            {
                string email = subscriber.Email;
                string subject = "Daily Newsletter";
                string emailBody = "This is your daily newsletter";

                await _emailHelper.SendEmail(email, subject, emailBody);
            }
        }
    }
}
