using EmailPOC.Events;
using EmailPOC.Interfaces;
using MediatR;

namespace EmailPOC.Helpers
{
    public class VerificationEmailHelper : IVerificationEmailHelper
    {
        private readonly IMediator _mediator;

        public VerificationEmailHelper(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task TriggerVerificationEmailEventAsync(string email)
        {
            var verificationEmail = new VerificationEmailEvent(email);
            await _mediator.Publish(verificationEmail);
        }
    }
}
