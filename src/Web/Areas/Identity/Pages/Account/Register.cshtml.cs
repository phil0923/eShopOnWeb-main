using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
   
    private readonly ILogger<RegisterModel> _logger;

    private readonly IIdentityServiceCaller _identityServiceCaller;

    public RegisterModel(
       ILogger<RegisterModel> logger,
       IIdentityServiceCaller identityServiceCaller)
    {
        _logger = logger;
        _identityServiceCaller = identityServiceCaller;
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

   
        var response = await _identityServiceCaller.Register(Input!.Email!, Input.Password!);

        if (response == null)
        {
            ModelState.AddModelError(string.Empty, "Registration failed. Please try again later.");
            return Page();
        }

      
        HttpContext.Session.SetString("JWT", response.Token);

        _logger.LogInformation("User created a new account through the Identity microservice.");

     
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, Input.Email!),
        new Claim(ClaimTypes.Email, Input.Email!)
    };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        // Redirect user to home or the provided return URL
        return LocalRedirect(returnUrl);
    }
}
