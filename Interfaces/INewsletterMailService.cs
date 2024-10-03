using EmailPOC.DataAccess.Entities;
using NAEPortal.Core.DataAccess;

namespace EmailPOC.Interfaces
{
    public interface INewsletterMailService
    {
        Task<List<FailedMailEntity>> HandleSendingMail(List<string> recepientsEmails, List<NewsletterMailEntity> mailsToSend);
        Task<List<int>> RetryWithFailedMails(NewsletterMailDbContext dbContext, List<FailedMailEntity> mailsToRetry);

    }
}
