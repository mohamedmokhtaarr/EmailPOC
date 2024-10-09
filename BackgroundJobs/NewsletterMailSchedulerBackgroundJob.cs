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
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;
using uSync.Core;

namespace EmailPOC.BackgroundJobs
{
    public class NewsletterMailSchedulerBackgroundJob : IRecurringBackgroundJob
    {
        private readonly IMemberService _memberService;
        private readonly IContentService _contentService;

        private readonly IEmailHelper _emailHelper;
        private readonly IMemberGroupService _memberGroupService;

        public NewsletterMailSchedulerBackgroundJob(IMemberService memberService, IContentService contentService, IEmailHelper emailHelper, IMemberGroupService memberGroupService)
        {
            _memberService = memberService;
            _contentService = contentService;
            _emailHelper = emailHelper;
            _memberGroupService = memberGroupService;
        }

        //private readonly int _runsEveryInSeconds;
        public TimeSpan Period => TimeSpan.FromMinutes(1);

        public event EventHandler PeriodChanged;

        protected T? GetPrimitivePropertyValue<T>(IProperty property)
        {
            IPropertyValue? propertyValue = property.Values.FirstOrDefault(v => v.EditedValue != null);
            Attempt<T>? conversion = propertyValue?.EditedValue?.TryConvertTo<T>();
            if (conversion is null || !conversion.Value.Success)
                return default;

            return conversion.Value.Result;
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
            IContent? mailContainer = _contentService.GetRootContent()
                        .FirstOrDefault(x=>x.ContentType.Alias.Equals(nameof(NewsletterList), StringComparison.OrdinalIgnoreCase));
            
            var mailResponse = _contentService.GetPagedChildren(mailContainer.Id,0,100,out long totalRecords).OrderByDescending(x=>x.PublishDate).FirstOrDefault();
            
            if (mailContainer == null)
            {
                return; // Handle error: rootNode not found
            }

            // Retrieve the "AdminResponse" property value from NewsletterMail
            string? adminResponse = GetPrimitivePropertyValue<string> (mailResponse.Properties["adminResponse"]);

            if (string.IsNullOrWhiteSpace(adminResponse))
            {
                return; // Handle error: adminResponse is null or empty
            }

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", "NewsletterTemplate.html");
            string emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

            // Replace the placeholder {AdminResponse} with the actual content
            string emailBody = emailTemplate.Replace("{AdminResponse}", adminResponse);
            foreach (var subscriber in subscribers)
            {
                string email = subscriber.Email;
                string subject = "Newsletter Subscription Test";

                bool success = await _emailHelper.SendEmail(email, subject, emailBody);
                if (!success)
                {
                    // Log error or handle failed email
                }
            }


        }
    }
}


