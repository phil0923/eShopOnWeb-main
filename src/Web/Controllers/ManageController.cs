using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;
using Microsoft.eShopWeb.Web.ViewModels.Manage;

namespace Microsoft.eShopWeb.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize] // Controllers that mainly require Authorization still use Controller/View; other pages use Pages
[Route("[controller]/[action]")]
public class ManageController : Controller
{
   
    private readonly IAppLogger<ManageController> _logger;
    private readonly UrlEncoder _urlEncoder;
    private readonly IConfiguration _config;
    private readonly IIdentityServiceCaller _identityServiceCaller;
    

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string RecoveryCodesKey = nameof(RecoveryCodesKey);

    public ManageController(
      IAppLogger<ManageController> logger,
      UrlEncoder urlEncoder,
      IConfiguration config, IIdentityServiceCaller identityServiceCaller)
    {
        _logger = logger;
        _urlEncoder = urlEncoder;
        _config = config;
        _identityServiceCaller = identityServiceCaller;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    [HttpGet]
    public async Task<IActionResult> MyAccount()
    {


        _logger.LogInformation("JWT cookie on request: {jwt}", Request.Cookies["JWT"] ?? "null");
        var user = await _identityServiceCaller.GetProfileAsync();
        if (user == null)
        {
            throw new ApplicationException($"Unable to load userprofile.");
        }

        

        var model = new IndexViewModel
        {
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsEmailConfirmed = user.IsEmailConfirmed,
            StatusMessage = StatusMessage
        };
       
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> MyAccount(IndexViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        UpdateProfileDTO dto = new UpdateProfileDTO() { Email=model.Email, PhoneNumber=model.PhoneNumber };

        var response= await _identityServiceCaller.UpdateProfileAsync(dto);

      
        if (response == false)
        {
            throw new ApplicationException($"Unable to update user.");
        }


        StatusMessage = "Your profile has been updated";
        return RedirectToAction(nameof(MyAccount));
    }

    [HttpPost]
    public async Task<IActionResult> SendVerificationEmail(IndexViewModel model)
    {
        if (!ModelState.IsValid ||model.Email==null )
        {
            return View(model);
        }

        SendVerificationEmailDTO dto = new SendVerificationEmailDTO() { Email = model.Email };

       var response=await _identityServiceCaller.SendVerificationAsync(dto);

        if (response == false)
        {
            throw new ApplicationException($"Unable to send verification email to {model.Email}.");
        }


        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToAction(nameof(MyAccount));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        
        bool response = await _identityServiceCaller.HasPasswordAsync();

      
        if (response==false)
        {
            return RedirectToAction(nameof(SetPassword));
        }

        var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid || model.NewPassword==null || model.OldPassword==null)
        {
            return View(model);
        }

        ChangePasswordDTO dto = new ChangePasswordDTO() { NewPassword = model.NewPassword, OldPassword = model.OldPassword };

        var response = await _identityServiceCaller.ChangePasswordAsync(dto);

    
        if (response==false)
        {
           
            return View(model);
        }

      
        _logger.LogInformation("User changed their password successfully.");
        StatusMessage = "Your password has been changed.";

        return RedirectToAction(nameof(ChangePassword));
    }

    [HttpGet]
    public async Task<IActionResult> SetPassword()
    {
        var hasPassword = await _identityServiceCaller.HasPasswordAsync();

      

        if (hasPassword)
        {
            return RedirectToAction(nameof(ChangePassword));
        }

        var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid || model.NewPassword==null)
        {
            return View(model);
        }

        SetPasswordDTO dto = new SetPasswordDTO() { NewPassword = model.NewPassword };

        var addPasswordResult = await _identityServiceCaller.SetPasswordAsync(dto);



        if (!addPasswordResult)
        {
            
            return View(model);
        }

      
        StatusMessage = "Your password has been set.";

        return RedirectToAction(nameof(SetPassword));
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLogins()
    {
        
        var externalLoginsDto = await _identityServiceCaller.GetExternalLoginsAsync();

    
        if (externalLoginsDto == null)
        {
            _logger.LogWarning("Failed to load external logins for user");
            StatusMessage = "Error: Could not load external logins at this time.";
            return RedirectToAction("Index", "Manage");
        }

    
        var model = new ExternalLoginsViewModel
        {
            CurrentLogins = externalLoginsDto.CurrentLogins
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
                .ToList(),

            OtherLogins = externalLoginsDto.OtherLogins
                .Select(p => new AuthenticationScheme(p.Name, p.DisplayName, null))
                .ToList(),

            ShowRemoveButton = externalLoginsDto.CanRemove,
            StatusMessage = StatusMessage
        };

       
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> LinkLogin(string provider)
    {
        // We no longer use SignInManager here.
        // Instead, we ask the Identity microservice to handle the redirect.

        // Instead of calling HttpClient, redirect the user’s browser directly
        var identityUrl = $"{_config["baseUrls:apigateWayBase"]}/identity/account/link-login?provider={Uri.EscapeDataString(provider)}";
        return Redirect(identityUrl);
        
    }
    

    [HttpGet]
    public IActionResult LinkLoginCallback([FromQuery] string? status = null)
    {
        

        // This endpoint doesn’t handle login anymore — just shows a result message
        if (status == "success")
            StatusMessage = "The external login was added successfully.";
        else
            StatusMessage = "There was an error linking the external login.";

        return RedirectToAction(nameof(ExternalLogins));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
    {

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        RemoveLoginDTO dto = new RemoveLoginDTO() { LoginProvider = model.LoginProvider, ProviderKey = model.ProviderKey };

        var response = await _identityServiceCaller.RemoveLoginAsync(dto);

        if (!response)
        {
            throw new ApplicationException($"Unexpected error occurred removing external login for user'.");
        }

      
        StatusMessage = "The external login was removed.";
        return RedirectToAction(nameof(ExternalLogins));
    }

    [HttpGet]
    public async Task<IActionResult> TwoFactorAuthentication()
    {
        var dtoResponse = await _identityServiceCaller.GetTwoFactorStatusAsync();

      ;
        if (dtoResponse == null)
        {
            throw new ApplicationException($"Unable to get TwoFactor Auth Status.");
        }

        var model = new TwoFactorAuthenticationViewModel
        {
            HasAuthenticator =dtoResponse.HasAuthenticator,
            Is2faEnabled = dtoResponse.Is2faEnabled,
            RecoveryCodesLeft = dtoResponse.RecoveryCodesLeft,
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Disable2faWarning()
    {


        var dtoResponse = await _identityServiceCaller.GetTwoFactorStatusAsync();

        ;
        if (dtoResponse == null)
        {
            throw new ApplicationException($"Unable to load user.");
        }
       

        if (!dtoResponse.Is2faEnabled)
        {
            throw new ApplicationException($"Unexpected error occured disabling 2FA for user");
        }

        return View(nameof(Disable2fa));
    }

    [HttpPost]
    public async Task<IActionResult> Disable2fa()
    {
        var response = await _identityServiceCaller.DisableTwoFactorAsync();


   
        if (!response)
        {
            throw new ApplicationException($"Unexpected error occured disabling 2FA for user");
        }

        _logger.LogInformation("User has disabled 2fa.");
        return RedirectToAction(nameof(TwoFactorAuthentication));
    }

    [HttpGet]
    public async Task<IActionResult> EnableAuthenticator()
    {
        var response = await _identityServiceCaller.GetTwoFactorSetupAsync();
       
        if (response == null)
        {
            throw new ApplicationException($"Unable to get twofactorsetup..");
        }



        var model = new EnableAuthenticatorViewModel();

        model.SharedKey = response.SharedKey;
        model.AuthenticatorUri = response.AuthenticatorUri;
 

        return View(model);
    }

    [HttpGet]
    public IActionResult ShowRecoveryCodes()
    {
        var recoveryCodes = (string[]?)TempData[RecoveryCodesKey];
        if (recoveryCodes == null)
        {
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        var model = new ShowRecoveryCodesViewModel { RecoveryCodes = recoveryCodes };
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
    {
   
     
        if (!ModelState.IsValid || model.Code==null)
        {
            await LoadSharedKeyAndQrCodeUriAsync(model);
            return View(model);
        }

        var verificationCode = model.Code?.Replace(" ", "").Replace("-", "") ?? "";

        var response = await _identityServiceCaller.EnableTwoFactorAsync(verificationCode);


        if (response==null)
        {
            ModelState.AddModelError("Code", "Verification code is invalid.");
            await LoadSharedKeyAndQrCodeUriAsync(model);
            return View(model);
        }

       
        _logger.LogInformation("User has enabled 2FA with an authenticator app.");
      
        TempData[RecoveryCodesKey] = response.RecoveryCodes;

        return RedirectToAction(nameof(ShowRecoveryCodes));
    }

    [HttpGet]
    public IActionResult ResetAuthenticatorWarning()
    {
        return View(nameof(ResetAuthenticator));
    }

    [HttpPost]
    public async Task<IActionResult> ResetAuthenticator()
    {

        var response= await _identityServiceCaller.ResetAuthenticatorAsync();

        if (response == false)
        {
            throw new ApplicationException($"Unable to reset authenticator.");
        }

       
        _logger.LogInformation("User has reset their authentication app key.");

        return RedirectToAction(nameof(EnableAuthenticator));
    }

    [HttpPost]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var response = await _identityServiceCaller.GenerateRecoveryCodesAsync();

     

        if (response==null)
        {
            throw new ApplicationException($"Cannot generate recovery codes for user as they do not have 2FA enabled.");
        }


        _logger.LogInformation("User has generated new 2FA recovery codes.");

        var model = new ShowRecoveryCodesViewModel { RecoveryCodes = response.RecoveryCodes };

        return View(nameof(ShowRecoveryCodes), model);
    }

    [HttpGet]
    public async Task<IActionResult> GenerateRecoveryCodesWarning()
    {
        var warning = await _identityServiceCaller.GetRecoveryCodesWarningAsync();

        if (warning == null)
        {
            // Could be unauthorized, 2FA not enabled, or service error
            StatusMessage = "Unable to load recovery code warning at this time.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

   
        return View(nameof(GenerateRecoveryCodesWarning));
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            AuthenticatorUriFormat,
            _urlEncoder.Encode("eShopOnWeb"),
            _urlEncoder.Encode(email),
            unformattedKey);
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync( EnableAuthenticatorViewModel model)
    {
        var response= await _identityServiceCaller.GetTwoFactorSetupAsync();

        if (response == null)
        {
            throw new ApplicationException($"Unable to get twofactorsetup.");
        }

        model.SharedKey = response.SharedKey;
        model.AuthenticatorUri = response.AuthenticatorUri;
    }

}
