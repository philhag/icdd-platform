using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.WebApplication.Environment;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace IcddWebApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [RegularExpression("^[a-zA-Z0-9]([._-](?![._-])|[a-zA-Z0-9]){1,18}[a-zA-Z0-9]$", ErrorMessage = "The Username must only consist of alphanumeric characters (a-zA-Z0-9), dot, underscore, or hyphen. The dot(.), underscore(_), or hyphen(-) must not be the first or last character and must not appear consecutively. The number of characters must be between 3 to 20.")]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long and requires one upper case letter, one number, and one special character.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "The {0} must be at least 8 characters long and requires one upper case letter, one number, and one special character.")]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new User { UserName = Input.Username, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                ViewData["Result"] = result;
                if (result.Succeeded)
                {
                    var addRole = await _userManager.AddToRoleAsync(user, "User");
                    var result2 = addRole.Succeeded;

                    if (result2)
                    {
                        _logger.LogInformation("User created a new account with password.");
                        Logger.Log($"Registration succeeded for {user.UserName} and {user.Email}!", Logger.MsgType.Error, "Register");
                        await _emailSender.SendEmailAsync("icdd-plattform@ruhr-uni-bochum.de", "Registration on ICDD Platform",
                           $"Registration succeeded for username {user.UserName} and email {user.Email}!");
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                             $"**This mail is auto-generated. Do not reply.**<br><br><br> Dear {user.UserName}, <br><br>you registered at the RUB ICDD Platform with this e-mail address <b>{user.Email}</b>.To use your account, please confirm your account on the RUB ICDD Platform by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br> <br> Best regards. <br><br>**This mail is auto-generated. Do not reply.**");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                await _emailSender.SendEmailAsync("icdd-plattform@ruhr-uni-bochum.de", "Registration failed",
                             $"Registration failed for {user.UserName} and {user.Email}: {result.Errors.First().Description}");

                Logger.Log($"Registration failed for {user.UserName} and {user.Email}: {result.Errors.First().Description}", Logger.MsgType.Error, "Register");
            }
            
            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
