using EmailPOC.Interfaces;
using EmailPOC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Controllers;

namespace EmailPOC.Controllers.Surface
{
    public class NewsletterSubscriptionController : SurfaceController
    {
        public static readonly string TempNewsletterMessageKey = "NewsletterMessage";
        private readonly ILogger<NewsletterSubscriptionController> _logger;
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;
        private readonly IEmailHelper _emailHelper;
        private readonly IValidationHelper _validation;

        public NewsletterSubscriptionController(
            IUmbracoContextAccessor umbracoContextAccessor,
            ILogger<NewsletterSubscriptionController> logger,
            IPublishedUrlProvider publishedUrlProvider,
            IUmbracoDatabaseFactory databaseFactory,
            IProfilingLogger profilingLogger,
            IEmailHelper emailHelper,
            ServiceContext services,
            AppCaches appCaches,
            IValidationHelper validation,
            IMemberGroupService memberGroupService)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _logger = logger;
            _emailHelper = emailHelper;

            if (services.MemberService is null)
            {
                _logger.LogError("Member Service was not Initiated");
                throw new ArgumentNullException(nameof(services.MemberService));
            }
            _memberService = services.MemberService;
            _validation = validation;
            _memberGroupService = memberGroupService;
        }



        public IActionResult Subscribe(NewsletterSubscriberViewModel model)
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Please enter your e-mail Address");
                return CurrentUmbracoPage();
            }

            if (!_validation.ValidateMail(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "You must enter the e-mail in the correct format");
                return CurrentUmbracoPage();
            }

            Guid newsletterGroupKey = new Guid("0a4a4c5c-ded9-4193-a39e-885dd78839dd");
            var newsletterGroup = _memberGroupService.GetById(newsletterGroupKey);

            if (newsletterGroup == null)
            {
                return CurrentUmbracoPage();
            }



            //check if the email subscribed or not
            IEnumerable<IMember> memberSubscibers = _memberService.FindMembersInRole(newsletterGroup.Name, model.Email, Umbraco.Cms.Core.Persistence.Querying.StringPropertyMatchType.Exact);

            if (memberSubscibers.Any())
            {
                ModelState.AddModelError(nameof(model.Email), "A Subscription was Found with the Same Email");
                return CurrentUmbracoPage();

            }

            //check if email is in members or guest

            var memberSubsciber = _memberService.GetByEmail(model.Email);
            if (memberSubsciber == null)
            {
                // create member as a guest 
                memberSubsciber = _memberService.CreateMemberWithIdentity(model.Email, model.Email, NewsletterSubscriber.ModelTypeAlias);
                memberSubsciber.SetValue(nameof(NewsletterSubscriber.SubscriptionDate).ToFirstLower(), DateTime.Now);
                _memberService.Save(memberSubsciber);

            }
            //assign to subscribers group

            _memberService.AssignRole(memberSubsciber.Email, newsletterGroup.Name);

            TempData[TempNewsletterMessageKey] = "You have successfully subscribed to the newsletter";
            return RedirectToCurrentUmbracoPage();
        }


        public async Task<IActionResult> Unsubscribe(NewsletterSubscriberViewModel model)
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Please enter your e-mail Address");
                return CurrentUmbracoPage();
            }

            if (!_validation.ValidateMail(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "You must enter the e-mail in the correct format");
                return CurrentUmbracoPage();
            }

            IMember? member = _memberService.GetByEmail(model.Email);
            if (member is null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email Not Found");
                return CurrentUmbracoPage();
            }

            string token = Guid.NewGuid().ToString().Replace("-", "");
            string unsubscribeUrl =
                $"{CurrentPage!.Url(mode: UrlMode.Absolute)}?{nameof(IMember.Email)}={member.Email}&{nameof(NewsletterSubscriber.UnsubscribeToken)}={token}";
            member.SetValue(nameof(NewsletterSubscriber.UnsubscribeToken).ToFirstLower(), token);

            string htmlTemplate = "This is Html Template to Unsubscribe News Letter";
            string mailSubject = "Unsubscribe from Newsletter";
            string mailText =
            string.Format(htmlTemplate, $"<a href='{unsubscribeUrl}'> {"Please use the following link to unsubscribe"} </a>");

		    Guid newsletterGroupKey = new Guid("0a4a4c5c-ded9-4193-a39e-885dd78839dd"); // Replace with your group key
		    IMemberGroup? newsletterGroup = _memberGroupService.GetById(newsletterGroupKey);
		    if (newsletterGroup == null)
		    {
		    	return CurrentUmbracoPage();
		    }
            if (await _emailHelper.SendEmail(
                    recepientEmail: member.Email,
                    mailSubject: mailSubject,
                    mailBody: mailText))
            {
                 var isSubscribed = _memberService.FindMembersInRole(newsletterGroup.Name, model.Email, Umbraco.Cms.Core.Persistence.Querying.StringPropertyMatchType.Exact);
				if (isSubscribed.Any())
				{
                    _memberService.DissociateRole(member.Email, newsletterGroup.Name); // Unassign the member from the group
				}
				TempData[nameof(NewsletterSubscriberViewModel)] = "Mailbox Check For Unsubscribing Process";
                _memberService.Save(member);
            }
            else
            {
                TempData[nameof(NewsletterSubscriberViewModel)] = "Something went wrong, please try again later";
            }
            
            Uri redirect = new(CurrentPage!.Url(mode: UrlMode.Absolute));
            return Redirect(redirect.AbsoluteUri);
        }


	}
}
