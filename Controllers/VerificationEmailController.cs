using EmailPOC.Helpers;
using EmailPOC.Interfaces;
using EmailPOC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmailPOC.Controllers
{
    [Route("verification")]

    public class VerificationEmailController : Controller
    {
        private readonly IVerificationEmailHelper verificationEmailHelper;

        public VerificationEmailController(IVerificationEmailHelper verificationEmailHelper)
        {
            this.verificationEmailHelper = verificationEmailHelper;
        }
        [HttpPost("verificationemail")]

        public async Task<IActionResult> VerificationEmail(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.Email.IsNullOrWhiteSpace())
                {
                    await verificationEmailHelper.TriggerVerificationEmailEventAsync(model.Email);
                    return View("PasswordResetConfirmation");

                }
            }

            return View(model);
        }
    }
}
