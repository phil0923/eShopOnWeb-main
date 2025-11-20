using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Ardalis.Result;
using Azure;
using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.SharedDTOs;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs.ExternalLoginDTO_s;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs.RecoveryCodesStatus;
using Newtonsoft.Json.Linq;
using NuGet.Common;

namespace Microsoft.eShopWeb.Web.Services;

public class IdentityServiceCaller : IIdentityServiceCaller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityServiceCaller> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    public IdentityServiceCaller(HttpClient httpClient, ILogger<IdentityServiceCaller> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _contextAccessor = httpContextAccessor;


    }

    private void AddJwtHeader()
    {
        var jwt = _contextAccessor.HttpContext?.Request.Cookies["JWT"];
        if (string.IsNullOrEmpty(jwt))
        {
            _logger.LogWarning("No JWT cookie found. Authorization header will not be attached.");
            return;
        }
        jwt = Uri.UnescapeDataString(jwt);
        jwt = jwt.Replace(" ", "").Trim('"').Trim();
        jwt = new string(jwt.Where(c => !char.IsControl(c)).ToArray());

        if (!jwt.Contains("."))
        {
            _logger.LogWarning("Malformed JWT detected. Skipping Authorization header attachment.");
            return;
        }
        _logger.LogInformation($"[CLEANED JWT]: {jwt.Substring(0, Math.Min(40, jwt.Length))}...");
       
        var currentAuth = _httpClient.DefaultRequestHeaders.Authorization?.Parameter;
        if (currentAuth != jwt)
        {
            _logger.LogInformation($"Attaching JWT token to Authorization header: {jwt} ");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);
        }
    }


    public async Task<bool> ChangePasswordAsync(ChangePasswordDTO dto)
    {
        AddJwtHeader();
        var result =await _httpClient.PostAsJsonAsync("account/change-password", dto);

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            result.RequestMessage?.RequestUri, result.StatusCode);
            return false;
        }


        return true;
    }

    public async Task<bool> DisableTwoFactorAsync()
    {
        AddJwtHeader();
        var result = await _httpClient.PostAsJsonAsync("account/2fa/disable", new { });

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            result.RequestMessage?.RequestUri, result.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<RecoveryCodesDTO?> EnableTwoFactorAsync(string code)
    {
        AddJwtHeader();
        var result = await _httpClient.PostAsJsonAsync("account/2fa/enable", new EnableTwoFactorDTO() { VerificationCode=code});

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            result.RequestMessage?.RequestUri, result.StatusCode);
            return null;
        }

      

        var dto = await result.Content.ReadFromJsonAsync<RecoveryCodesDTO>();

        if (dto == null)
        {
            return null;
        }
         

        return dto;



    }

    public async Task<UserProfileDTO?> GetProfileAsync()
    {
        AddJwtHeader();
        var result = await _httpClient.GetAsync("account/profile");



        if (!result.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            result.RequestMessage?.RequestUri, result.StatusCode);
            return null;
        }
        var profile = await result.Content.ReadFromJsonAsync<UserProfileDTO>();

        return profile;
    }

    public async Task<LoginResponseDTO?> Login(string email, string password)
    {
        try
        {
            _logger.LogInformation("Sending login request to {url}", _httpClient.BaseAddress);
            var response = await _httpClient.PostAsJsonAsync("auth/login", new UserLoginDTO { Email = email, Password = password });
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed login attempt for {email}", email);
                return null;
            }

            var content = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();

            if (string.IsNullOrEmpty(content?.Token))
            {
                _logger.LogWarning("IdentityService returned no token for {email}", email);
                return null;
            }

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot reach Identity Service for {Email}", email);
            return null; // <— prevent crash
        }
    }

    public async Task<RegisterResponseDTO?> Register(string email, string password)
    {
       
        try
        {


            _logger.LogInformation("Sending registration request to {url}", _httpClient.BaseAddress);
            var response = await _httpClient.PostAsJsonAsync("auth/register", new UserRegistrationDTO { Email = email, Password = password });
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed registration attempt for {email}", email);
                return null;
            }



            var content = await response.Content.ReadFromJsonAsync<RegisterResponseDTO>();

            if (string.IsNullOrEmpty(content?.Token))
            {
                _logger.LogWarning("IdentityService returned no token for {email}", email);
                return null;
            }



            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot reach Identity Service for {Email}", email);
            return null; // <— prevent crash
        }
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileDTO dto)
    {
        AddJwtHeader();
        try
        {


            _logger.LogInformation("Sending updateprofile request to {url}", _httpClient.BaseAddress);
            var response = await _httpClient.PutAsJsonAsync("account/profile", dto);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed updateprofile attempt for {email}", dto.Email);
                return false;
            }


            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot reach Identity Service for {Email}", dto.Email);
            return false; // <— prevent crash
        }
    }


    public async Task<TwoFactorStatusDTO?> GetTwoFactorStatusAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.GetAsync("account/2fa/status");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            response.RequestMessage?.RequestUri, response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TwoFactorStatusDTO>();
    }

    public async Task<bool> ResetAuthenticatorAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.PostAsJsonAsync("account/2fa/reset-authenticator", new { });
        return response.IsSuccessStatusCode;
    }

    public async Task<TwoFactorSetupDTO?> GetTwoFactorSetupAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.GetAsync("account/2fa/setup");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            response.RequestMessage?.RequestUri, response.StatusCode);
            return null;
        }
        return await response.Content.ReadFromJsonAsync<TwoFactorSetupDTO>();
    }
    public async Task<RecoveryCodesDTO?> GenerateRecoveryCodesAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.PostAsJsonAsync("account/2fa/generate-recovery-codes", new { });
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            response.RequestMessage?.RequestUri, response.StatusCode);
            return null;
        }
        return await response.Content.ReadFromJsonAsync<RecoveryCodesDTO>();
    }

    public async Task<bool> HasPasswordAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.GetAsync("account/has-password");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            response.RequestMessage?.RequestUri, response.StatusCode);
            return false;
        }
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("hasPassword").GetBoolean();
    }


    public async Task<bool> SetPasswordAsync(SetPasswordDTO dto)
    {
        AddJwtHeader();
        var response = await _httpClient.PostAsJsonAsync("account/set-password", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendVerificationAsync(SendVerificationEmailDTO dto)
    {
        AddJwtHeader();
        var response = await _httpClient.PostAsJsonAsync("account/send-verification", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("account/confirm-email", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<ExternalLoginsDTO?> GetExternalLoginsAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.GetAsync("account/external-logins");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            response.RequestMessage?.RequestUri, response.StatusCode);
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ExternalLoginsDTO>();
    }

    public async Task<bool> RemoveLoginAsync(RemoveLoginDTO dto)
    {
        AddJwtHeader();
        var response = await _httpClient.PostAsJsonAsync("account/remove-login", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<RecoveryCodesStatusDTO?> GetRecoveryCodesStatusAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.GetAsync("account/recovery-codes");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            response.RequestMessage?.RequestUri, response.StatusCode);
            return null;
        }
        return await response.Content.ReadFromJsonAsync<RecoveryCodesStatusDTO>();
    }

    public async Task<RecoveryCodesWarningDTO?> GetRecoveryCodesWarningAsync()
    {
        AddJwtHeader();
        var response = await _httpClient.GetAsync("account/recovery-codes/warning");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Request to {Path} failed with status {StatusCode}",
            response.RequestMessage?.RequestUri, response.StatusCode);
            return null;
        }
        return await response.Content.ReadFromJsonAsync<RecoveryCodesWarningDTO>();
    }





}
