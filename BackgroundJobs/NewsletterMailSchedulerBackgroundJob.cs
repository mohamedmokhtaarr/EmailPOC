using EmailPOC.DataAccess.Entities;
using EmailPOC.Extensions;
using EmailPOC.Interfaces;
using EmailPOC.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NAEPortal.Core.DataAccess;
using Serilog;
using System.Diagnostics;
using System.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace EmailPOC.BackgroundJobs
{
    public class NewsletterMailSchedulerBackgroundJob : IRecurringBackgroundJob
    {
        private readonly IMemberService _memberService;
        private readonly IEmailHelper _emailHelper;
        private readonly IMemberGroupService _memberGroupService;

       //private readonly int _runsEveryInSeconds;
        public TimeSpan Period => TimeSpan.FromMinutes(5);

        public event EventHandler PeriodChanged;

        public NewsletterMailSchedulerBackgroundJob
            (IMemberService memberService,
            IEmailHelper emailHelper,
            IMemberGroupService memberGroupService)
        {
            _memberService = memberService;
            _emailHelper = emailHelper;
            _memberGroupService = memberGroupService;
        }
        public async Task RunJobAsync()
        {
            Guid newsletterGroupKey = new Guid("0a4a4c5c-ded9-4193-a39e-885dd78839dd"); // Replace with your group key
            var newsletterGroup = _memberGroupService.GetById(newsletterGroupKey);

            if (newsletterGroup == null)
            {
                return; // Handle error: no group found
            }

            var subscribers = _memberService.GetMembersInRole(newsletterGroup.Name); // Get all subscribers

            foreach (var subscriber in subscribers)
            {
                string email = subscriber.Email;
                string subject = "Newsletter Subscription Test";
                string body = "Hi there, it is testing for newsletter subscription.";

                bool success = await _emailHelper.SendEmail(email, subject, body);
                if (!success)
                {
                    // Log error or handle failed email
                }
            }


        }
    }
}


