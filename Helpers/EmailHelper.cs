using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;using Microsoft.Extensions.Options;
using static EmailPOC.Helpers.EmailHelper;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Services;
using EmailPOC.Interfaces;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Web.Common.PublishedModels;
using File = System.IO.File;

namespace EmailPOC.Helpers
{
	
		internal class EmailHelper : IEmailHelper
		{
			private GlobalSettings GlobalSettings { get; }
			private IMemberService MemberService { get; }
			private IEmailSender EmailSender { get; }
			private ILogger<EmailHelper> Logger { get; }
			private IWebHostEnvironment WebHostEnvironment { get; }

			public EmailHelper(
				IOptions<GlobalSettings> globalSettings,
				IMemberService memberService,
				IEmailSender emailSender,
				ILogger<EmailHelper> logger,
				IWebHostEnvironment webHostEnvironment)
			{

				GlobalSettings = globalSettings.Value;
				MemberService = memberService;
				EmailSender = emailSender;
				Logger = logger;
				WebHostEnvironment = webHostEnvironment;
			}

			public async Task<bool> SendEmail(string mailSubject, string mailBody, string? mailTemplate = null, List<string>? attachmentsFilePathList = null)
			{
				if (!ValidateSmtpSettingsExistence() || !ValidateMailContentExistence(mailSubject, mailBody))
					return false;

				string[]? mailToList = MemberService.GetAllMembers()?.Select(m => m.Email)?.ToArray();
				if (mailToList == null)
					return false;

				return await SendMail(mailToList, mailSubject, GetEmailBody(mailBody, mailTemplate), attachmentsFilePathList);
			}

			/// <summary>
			/// Send a Single Email to a Specified Recepient Email, without attachements
			/// </summary>
			/// <param name="recepientEmail"></param>
			/// <param name="mailSubject"></param>
			/// <param name="mailBody"></param>
			/// <param name="mailTemplate"></param>
			/// <param name="attachmentsFilePathList"></param>
			/// <returns>true if email did not cause an exception, otherwise false</returns>
			public async Task<bool> SendEmail(string recepientEmail, string mailSubject, string mailBody)
			{
				if (!ValidateSmtpSettingsExistence() || !ValidateMailContentExistence(mailSubject, mailBody))
					return false;

				try
				{
					await EmailSender.SendAsync(
						message: new EmailMessage(GlobalSettings!.Smtp!.From, recepientEmail, mailSubject, mailBody, true),
						emailType: nameof(NewsletterMail));

					return true;
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, "Email Sending Caused an Exception");
					return false;
				}
			}

			private bool ValidateSmtpSettingsExistence()
			{
				SmtpSettings? smtpSettings = GlobalSettings?.Smtp;
				string nonProvidedSettings = string.Empty;

				if (smtpSettings is null) return false;

				if (string.IsNullOrWhiteSpace(smtpSettings.From))
					nonProvidedSettings += nameof(smtpSettings.From) + " ,";

				if (string.IsNullOrWhiteSpace(smtpSettings.Username))
					nonProvidedSettings += nameof(smtpSettings.Username) + " ,";

				if (string.IsNullOrWhiteSpace(smtpSettings.Password))
					nonProvidedSettings += nameof(smtpSettings.Password) + " ,";

				if (string.IsNullOrWhiteSpace(smtpSettings.Host))
					nonProvidedSettings += nameof(smtpSettings.Host) + " ,";

				if (smtpSettings.Port <= default(int))
					nonProvidedSettings += nameof(smtpSettings.Port) + " ,";

				if (!string.IsNullOrWhiteSpace(nonProvidedSettings))
				{
					Logger.LogWarning($"Non provided SMTP Settings are: {nonProvidedSettings}");
					return false;
				}

				return true;
			}
			private bool ValidateMailContentExistence(string mailSubject, string mailBody)
			{
				if (string.IsNullOrWhiteSpace(mailSubject) || string.IsNullOrWhiteSpace(mailBody))
				{
					Logger.LogWarning($"Cannot send email due to empty mail body or subject");
					return false;
				}

				return true;
			}

			private string GetEmailBody(string mailBody, string? mailTemplate = null) =>
				mailTemplate != null ? string.Format(mailTemplate, mailBody) : mailBody;

			public async Task<bool> SendMail(string[] mailToList, string mailSubject, string mailBody, List<string>? attachmentsList)
			{
				IEnumerable<EmailMessageAttachment>? emailMessageAttachmentList = null;

				if (attachmentsList?.Count > default(int))
					emailMessageAttachmentList = attachmentsList.Select(filePath =>
					{
						var physicalFilePath = WebHostEnvironment.WebRootPath + filePath.Replace('/', '\\');
						return new EmailMessageAttachment(File.OpenRead(physicalFilePath), Path.GetFileName(filePath));
					}).ToList();

				try
				{
					EmailMessage emailMessage = new(GlobalSettings!.Smtp!.From, mailToList,
						null, null, null, mailSubject, mailBody, true, emailMessageAttachmentList);

					await EmailSender.SendAsync(emailMessage, emailType: "Newsletter");

					return true;
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, "Email Sending Caused an Exception");
					return false;
				}
			}

			public async Task<bool> SendIndividualMails(string[] mailToList, string mailSubject, string mailBody, List<string>? attachmentsList)
			{
				if (!ValidateSmtpSettingsExistence() || !ValidateMailContentExistence(mailSubject, mailBody))
					return false;

				IEnumerable<EmailMessageAttachment>? emailMessageAttachmentList = null;
				if (attachmentsList?.Count > default(int))
					emailMessageAttachmentList = attachmentsList.Select(filePath =>
					{
						var physicalFilePath = WebHostEnvironment.WebRootPath + filePath.Replace('/', '\\');
						return new EmailMessageAttachment(File.OpenRead(physicalFilePath), Path.GetFileName(filePath));
					}).ToList();

				try
				{
					List<Task> tasks = [];
					for (int i = 0; i < mailToList.Length; i++)
					{
						EmailMessage message = new(GlobalSettings!.Smtp!.From, mailToList[i], mailSubject, mailBody, true);
						tasks.Add(
							Task.Run(() => EmailSender.SendAsync(message, emailType: "Newsletter"))
						);
					}

					await Task.WhenAll(tasks);
					return true;
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, "Email Sending Caused an Exception");
					return false;
				}
			}
		}
	}

