using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.SharedDTOs;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;

namespace Microsoft.eShopWeb.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
  
    private readonly ILogger<LoginModel> _logger;
    private readonly IBasketService _basketService;
    private readonly IIdentityServiceCaller _identityServiceCaller;
    private readonly HttpClient _httpClient;
    public LoginModel(ILogger<LoginModel> logger, IBasketService basketService, IIdentityServiceCaller identityServiceCaller, HttpClient httpClient)
    {
        _logger = logger;
        _basketService = basketService;
        _identityServiceCaller = identityServiceCaller;
        _httpClient = httpClient; 
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl = returnUrl ?? Url.Content("~/");

        // Clear any leftover cookies from previous sessions
        await HttpContext.SignOutAsync();

        // No external logins for microservice setup
        ExternalLogins = new List<AuthenticationScheme>();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
            LoginResponseDTO? result = await _identityServiceCaller.Login(Input!.Email!, Input!.Password!);


            if (result == null)
            {   
                ModelState.AddModelError(string.Empty, "Login service unavailable or invalid credentials.");

                return Page();
            }

            if (result.Succeeded == false)
            {
                if (result.RequiresTwoFactor)
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input?.RememberMe });

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
            // If we get here — successful login
            _logger.LogInformation("User logged in successfully via IdentityService.");


            // Decode JWT
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            // Create cookie from claims
            var claimsIdentity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            //// Keeping JWT for microservice calls
            //HttpContext.Session.SetString("JWT", result.Token);

            Response.Cookies.Append("JWT", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // true in production (HTTPS)
                SameSite = SameSiteMode.Lax,
                Path = "/", // make sure it’s sent to all routes
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

            _logger.LogInformation("JWT saved to session: {token}", result.Token[..20] + "...");

            _logger.LogInformation("User logged in.");
            await TransferAnonymousBasketToUserAsync(Input?.Email);
            return LocalRedirect(returnUrl);
            
          
           

        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private async Task TransferAnonymousBasketToUserAsync(string? userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            _logger.LogWarning("Attempted to transfer anonymous basket, but username was null or empty.");
            return;
        }
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            var anonymousId = Request.Cookies[Constants.BASKET_COOKIENAME];
            if (Guid.TryParse(anonymousId, out var _))
            {
                Guard.Against.NullOrEmpty(userName, nameof(userName));
                await _basketService.TransferBasketAsync(anonymousId, userName);
            }
            Response.Cookies.Delete(Constants.BASKET_COOKIENAME);      
        }
    }
}
