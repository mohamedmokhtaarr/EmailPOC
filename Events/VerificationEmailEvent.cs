using MediatR;

namespace EmailPOC.Events
{
    public class VerificationEmailEvent : INotification
    {
        public string Email { get; set; }
        public VerificationEmailEvent(string email)
        {
            Email = email;
        }
    }
}
