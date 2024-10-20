using EmailPOC.Events;
using EmailPOC.Interfaces;
using MediatR;

namespace EmailPOC.Helpers
{
    public class ResetPasswordHelper : IResetPasswordHelper
    {
        private readonly IMediator _mediator;

        public ResetPasswordHelper(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task TriggerResetPasswordEventAsync(string email)
        {
            var resetPasswordEvent = new ResetPasswordEvent(email);
            await _mediator.Publish(resetPasswordEvent);
        }
    }
}
