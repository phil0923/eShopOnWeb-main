using System.Security.Claims;
using BlazorShared.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.eShopWeb.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ITokenClaimsService _tokenClaimsService;
    private readonly ILogger<UserController> _logger;
    private readonly IMemoryCache _cache;

    public UserController(
                        
                          ILogger<UserController> logger,
                          IMemoryCache cache)
    {
       
        _logger = logger;
        _cache = cache;
    }

    [HttpGet]
    [Authorize]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentUser() =>
        Ok(await CreateUserInfo(User));

    [Route("Logout")]
    [HttpPost]
    [Authorize]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        // 1. Remove the local cookie so the user is signed out in this app
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 2. (Optional) If you want to inform the Identity service to invalidate the JWT
        // var jwt = HttpContext.Session.GetString("JWT");
        // if (!string.IsNullOrEmpty(jwt))
        // {
        //     await _identityServiceCaller.LogoutAsync(jwt); // implement this in your identity service if needed
        // }

        // 3. (Optional) Manage your cache entry if you still use that system
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var identityKey = Request.Cookies[ConfigureCookieSettings.IdentifierCookieName];
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(identityKey))
        {
            _cache.Set($"{userId}:{identityKey}", identityKey, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(ConfigureCookieSettings.ValidityMinutesPeriod)
            });
        }

        _logger.LogInformation("User logged out.");

        // 4. Redirect to home or return OK
        return Ok(new { message = "Logged out successfully" });
    }

    private async Task<UserInfo> CreateUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identity == null || claimsPrincipal.Identity.Name == null || !claimsPrincipal.Identity.IsAuthenticated)
        {
            return UserInfo.Anonymous;
        }

        var userInfo = new UserInfo
        {
            IsAuthenticated = true
        };

        if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            userInfo.NameClaimType = claimsIdentity.NameClaimType;
            userInfo.RoleClaimType = claimsIdentity.RoleClaimType;
        }
        else
        {
            userInfo.NameClaimType = "name";
            userInfo.RoleClaimType = "role";
        }

        if (claimsPrincipal.Claims.Any())
        {
            var claims = new List<ClaimValue>();
            var nameClaims = claimsPrincipal.FindAll(userInfo.NameClaimType);
            foreach (var claim in nameClaims)
            {
                claims.Add(new ClaimValue(userInfo.NameClaimType, claim.Value));
            }

            foreach (var claim in claimsPrincipal.Claims.Except(nameClaims))
            {
                claims.Add(new ClaimValue(claim.Type, claim.Value));
            }

            userInfo.Claims = claims;
        }

        var token = await _tokenClaimsService.GetTokenAsync(claimsPrincipal.Identity.Name);
        userInfo.Token = token;

        return userInfo;
    }
}
