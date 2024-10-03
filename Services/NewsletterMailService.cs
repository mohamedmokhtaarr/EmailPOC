using EmailPOC.BackgroundJobs;
using EmailPOC.DataAccess.Entities;
using EmailPOC.Interfaces;
using EmailPOC.Settings;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NAEPortal.Core.DataAccess;
using Serilog;
using EmailPOC.Extensions;

namespace EmailPOC.Services
{
    public class NewsletterMailService : INewsletterMailService
    {
        private readonly IEmailHelper _emailHelper;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRazorViewEngine _viewEngine;
        private readonly NewsletterSettings _newsletterSettings;
        private readonly ITempDataProvider _tempDataProvider;

        public NewsletterMailService(IEmailHelper emailHelper, IServiceProvider serviceProvider, IRazorViewEngine viewEngine, NewsletterSettings newsletterSettings, ITempDataProvider tempDataProvider)
        {
            _emailHelper = emailHelper;
            _serviceProvider = serviceProvider;
            _viewEngine = viewEngine;
            _newsletterSettings = newsletterSettings;
            
            Log.Logger = new LoggerConfiguration().CreateNewsletterLogger(_newsletterSettings.LoggerPath);
            _serviceProvider = serviceProvider;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
        }

        public async Task<List<int>> RetryWithFailedMails(NewsletterMailDbContext dbContext, List<FailedMailEntity> mailsToRetry)
        {
            List<int> successfulMails = [];
            foreach (FailedMailEntity failedMail in mailsToRetry)
            {
                bool isDead = failedMail.Retries >= _newsletterSettings.MaximumNumberOfRetries;
                bool shouldWait = failedMail.FailDate < DateTime.Now.AddSeconds(_newsletterSettings.RetryThrottleInSeconds);

                if (shouldWait)
                {
                    continue;
                }

                if (isDead)
                {
                    Log.Logger.Warning("'Dead' Mail Detected! {mail}, skipping..", failedMail.ToString());
                    successfulMails.Add(failedMail.Id);
                    continue;
                }

                NewsletterMailEntity? mail = await dbContext.NewsletterMails
                    .AsNoTracking().FirstOrDefaultAsync(x => x.Id == failedMail.NewsletterMailId);
                if (mail == null)
                {
                    Log.Information("Skipping, Reason: Mail Reference was not Found");
                    continue;
                }

                (string subject, string body) = await GetMailBody(mail);
                if (string.IsNullOrWhiteSpace(body) || string.IsNullOrWhiteSpace(subject))
                {
                    Log.Information("Skipping, Reason: Empty Mail Body");
                    continue;
                }

                bool success = await _emailHelper.SendEmail(
                    recepientEmail: failedMail.RecepientEmail,
                    mailSubject: subject,
                    mailBody: body);

                if (success)
                {
                    successfulMails.Add(failedMail.Id);
                }
                else
                {
                    Log.Warning("Failed sending mail to {recipientEmail}, email queued for retrying...", failedMail.RecepientEmail);
                    failedMail.FailDate = DateTime.Now;
                    failedMail.Retries++;
                }
            }

            return successfulMails;
        }

        public async Task<List<FailedMailEntity>> HandleSendingMail(List<string> recepientsEmails, List<NewsletterMailEntity> mailsToSend)
        {
            List<FailedMailEntity> failedMails = [];
            foreach (NewsletterMailEntity mail in mailsToSend)
            {
                (string subject, string body) = await GetMailBody(mail);
                if (string.IsNullOrWhiteSpace(body) || string.IsNullOrWhiteSpace(subject))
                {
                    Log.Warning("Background Job Aborted: {0}, Reason: Empty Mail Body",
                        nameof(NewsletterMailSchedulerBackgroundJob));
                    continue;
                }

                foreach (string email in recepientsEmails)
                {
                    bool success = await _emailHelper.SendEmail(
                        recepientEmail: email,
                        mailSubject: subject,
                        mailBody: body);

                    if (!success)
                    {
                        failedMails.Add(new FailedMailEntity()
                        {
                            RecepientEmail = email,
                            FailDate = DateTime.Now,
                            NewsletterMailId = mail.Id,
                            Retries = 0
                        });
                    }
                }
                mail.SentDate = DateTime.Now;
            }

            return failedMails;
        }

        private async Task<(string subject, string body)> GetMailBody(NewsletterMailEntity mail)
        {
            DefaultHttpContext httpContext = new()
            {
                RequestServices = _serviceProvider
            };

            ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
            string viewName = "Templates/NewsletterMailTemplateRazor";
            IView? view = default;
            ViewEngineResult getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                view = getViewResult.View;
            }

            ViewEngineResult findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                view = findViewResult.View;
            }

            if (view is null)
            {
                return (string.Empty, string.Empty);
            }

            await using StringWriter output = new();
            ViewContext viewContext = new(
                actionContext,
                view,
                new ViewDataDictionary<NewsletterMailEntity>(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary())
                {
                    Model = mail
                },
                new TempDataDictionary(
                    actionContext.HttpContext,
                    _tempDataProvider),
                output,
                new HtmlHelperOptions()
            );

            await view.RenderAsync(viewContext);
            return ("Subject", output.ToString());
        }

    }
}
