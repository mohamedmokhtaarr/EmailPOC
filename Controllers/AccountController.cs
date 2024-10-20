using EmailPOC.Events;
using EmailPOC.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmailPOC.Controllers
{
    [Route("account")]

    public class AccountController : Controller
    {
        private readonly IMediator _mediator;
        public static readonly string TempResetPasswordMessageKey = "ResetPasswordMessage";

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Email != null)
                {
                string userEmail = model.Email;
                var resetPasswordEvent = new ResetPasswordEvent(userEmail);

                // Fire the event
                await _mediator.Publish(resetPasswordEvent);
                }
                
                // Redirect to a confirmation page after success
                return View("PasswordResetConfirmation");
            }

            // If validation fails, return the view with the same model
            return View(model);
        }

    }

}
