using EmailPOC.BackgroundJobs;
using EmailPOC.Events;
using EmailPOC.Interfaces;
using MediatR;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace EmailPOC.Handlers
{
    public class ScheduledEmailHandler : INotificationHandler<ScheduledEmailEvent>
    {
        private readonly IEmailHelper _emailHelper;
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;
        private readonly ILogger<NewsletterMailSchedulerBackgroundJob> _logger;
        public ScheduledEmailHandler(IEmailHelper emailHelper, IMemberService memberService, IMemberGroupService memberGroupService, ILogger<NewsletterMailSchedulerBackgroundJob> logger)
        {
            _emailHelper = emailHelper;
            _memberService = memberService;
            _memberGroupService = memberGroupService;
            _logger = logger;
        }

        public async Task Handle(ScheduledEmailEvent notification, CancellationToken cancellationToken)
        {
            var newsletterGroupKey = new Guid("0a4a4c5c-ded9-4193-a39e-885dd78839dd");
            var newsletterGroup = _memberGroupService.GetById(newsletterGroupKey);

            if (newsletterGroup == null)
            {
                _logger.LogError("Newsletter group not found.");
                return;
            }

            var newsLetterSubscribers = _memberService.GetMembersInRole(newsletterGroup.Name).ToList();

            if (newsLetterSubscribers.Count == 0)
            {
                _logger.LogInformation("No subscribers found in the newsletter group.");
                return;
            }

            IEnumerable<IMember> subscribers;

            switch (notification.MemberType)
            {
                case "MOHP Member":
                    subscribers = _memberService.GetMembersInRole(nameof(NewsletterMohp))
                        .Where(m => newsLetterSubscribers.Any(x => x.Email == m.Email)); 
                    break;
                case "Citizen":
                    subscribers = _memberService.GetMembersInRole(nameof(NewsletterCitizen))
                        .Where(m => newsLetterSubscribers.Any(x => x.Email == m.Email));
                    break;
                case "Guest":
                    subscribers = _memberService.GetMembersInRole(nameof(NewsletterSubscriber))
                        .Where(m => newsLetterSubscribers.Any(x => x.Email == m.Email));
                    break;
                default:
                    subscribers = _memberService.GetAllMembers()
                        .Where(m => newsLetterSubscribers.Any(x => x.Email == m.Email)); 
                    break;
            }

            if (!subscribers.Any())
            {
                _logger.LogInformation("No subscribers found for the selected member type in the newsletter group.");
                return;
            }

            foreach (var subscriber in subscribers)
            {
                string email = subscriber.Email;
                string subject = "Scheduled Newsletter";
                string emailBody = "This is a scheduled newsletter based on your membership type";

                await _emailHelper.SendEmail(email, subject, emailBody);
            }
        }


    }
}