using MediatR;

namespace EmailPOC.Events
{
    public class ResetPasswordEvent : INotification
    {
        public string Email { get; set; }

        public ResetPasswordEvent(string email)
        {
            Email = email;
        }
    }

}
