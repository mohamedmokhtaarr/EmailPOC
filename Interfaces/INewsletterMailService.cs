using EmailPOC.DataAccess;
using EmailPOC.DataAccess.Entities;

namespace EmailPOC.Interfaces
{
    public interface INewsletterMailService
    {
        Task<List<FailedMailEntity>> HandleSendingMail(List<string> recepientsEmails, List<NewsletterMailEntity> mailsToSend);
        Task<List<int>> RetryWithFailedMails(NewsletterMailDbContext dbContext, List<FailedMailEntity> mailsToRetry);

    }
}
