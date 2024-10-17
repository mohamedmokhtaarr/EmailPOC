using MediatR;

namespace EmailPOC.Events
{
    public class SendNewsletterEvent : INotification
    {

        public string Subject { get; }
        public string Body { get; }


        public SendNewsletterEvent(string subject, string body)
        {
            Subject = subject;
            Body = body;
        }
    }
}
