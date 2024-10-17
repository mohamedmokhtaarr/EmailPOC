using EmailPOC.DataAccess.Entities;
using EmailPOC.Events;
using EmailPOC.Extensions;
using EmailPOC.Interfaces;
using EmailPOC.Settings;
using MediatR;
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
        private readonly IMediator _mediator;
        private readonly IMemberGroupService _memberGroupService;
        private readonly ILogger<NewsletterMailSchedulerBackgroundJob> _logger;

        public NewsletterMailSchedulerBackgroundJob
            (IMemberService memberService,
            IContentService contentService,
            IMemberGroupService memberGroupService,
            ILogger<NewsletterMailSchedulerBackgroundJob> logger,
            IMediator mediator)
        {
            _memberService = memberService;
            _contentService = contentService;
            _memberGroupService = memberGroupService;
            _logger = logger;
            _mediator = mediator;
        }

        
        public TimeSpan Period => TimeSpan.FromHours(1);

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
            try
            {

                IContent? mailContainer = _contentService.GetRootContent()
                            .FirstOrDefault(x => x.ContentType.Alias.Equals(nameof(NewsletterList), StringComparison.OrdinalIgnoreCase));

                if (mailContainer == null)
                {
                    _logger.LogError("No mail response found.");
                    return;
                }
               
                var mailResponse = _contentService.GetPagedChildren(mailContainer.Id, 0, 100, out long totalRecords)
                    .OrderByDescending(x => x.PublishDate).FirstOrDefault();

                if (mailResponse == null)
                {
                    _logger.LogError("No mail response found.");
                    return;
                }

               
                // Retrieve the "AdminResponse" property value from NewsletterMail
                string? adminResponse = GetPrimitivePropertyValue<string>(mailResponse.Properties["adminResponse"]);

                if (string.IsNullOrWhiteSpace(adminResponse))
                {
                    _logger.LogError("Admin response is empty.");
                    return;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", "NewsletterTemplate.html");
                string emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
                string emailBody = emailTemplate.Replace("{AdminResponse}", adminResponse);


                var sendNewsletterEvent = new SendNewsletterEvent("Newsletter Subscription Test", emailBody);
                await _mediator.Publish(sendNewsletterEvent);

                _logger.LogInformation("SendNewsletterEvent fired successfully.");
            }


            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running the newsletter background job.");
            }
        }
    }
}


