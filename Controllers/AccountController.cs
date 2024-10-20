using EmailPOC.Events;
using EmailPOC.Interfaces;
using EmailPOC.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmailPOC.Controllers
{
    [Route("account")]

    public class AccountController : Controller
    {
        private readonly IResetPasswordHelper _resetPasswordHelper;

        public AccountController(IResetPasswordHelper resetPasswordHelper)
        {
            _resetPasswordHelper = resetPasswordHelper;
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.Email.IsNullOrWhiteSpace())
                {
                await _resetPasswordHelper.TriggerResetPasswordEventAsync(model.Email);
                return View("PasswordResetConfirmation");

                }
            }

            return View(model);
        }

    }

}
