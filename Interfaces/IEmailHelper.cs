namespace EmailPOC.Interfaces
{
	public interface IEmailHelper
	{
		public Task<bool> SendEmail(string mailSubject, string mailBody, string? mailTemplate = null, List<string>? attachmentsFilePathList = null);
		public Task<bool> SendEmail(string recepientEmail, string mailSubject, string mailBody);
		public Task<bool> SendMail(string[] mailToList, string mailSubject, string mailBody, List<string>? attachmentsList);
		public Task<bool> SendIndividualMails(string[] mailToList, string mailSubject, string mailBody, List<string>? attachmentsList);
	}
}
