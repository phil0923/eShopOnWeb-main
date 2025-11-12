using Microsoft.eShopWeb.Web.SharedDTOs;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs.ExternalLoginDTO_s;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs.RecoveryCodesStatus;

namespace Microsoft.eShopWeb.Web.Interfaces;

public interface IIdentityServiceCaller
{
    Task<LoginResponseDTO?> Login(string email, string password);

    Task<RegisterResponseDTO?> Register(string email, string password);

    Task<UserProfileDTO?> GetProfileAsync();
    Task<bool> UpdateProfileAsync(UpdateProfileDTO dto);
    Task<bool> ChangePasswordAsync(ChangePasswordDTO dto);
    Task<RecoveryCodesDTO?> EnableTwoFactorAsync(string code);
    Task<bool> DisableTwoFactorAsync();
    Task<TwoFactorStatusDTO?> GetTwoFactorStatusAsync();
    Task<bool> ResetAuthenticatorAsync();

    Task<TwoFactorSetupDTO?> GetTwoFactorSetupAsync();

    Task<RecoveryCodesDTO?> GenerateRecoveryCodesAsync();
    Task<bool> HasPasswordAsync();

    Task<bool> SetPasswordAsync(SetPasswordDTO dto);
    Task<bool> SendVerificationAsync(SendVerificationEmailDTO dto);

    Task<bool> ConfirmEmailAsync(ConfirmEmailDTO dto);

    Task<ExternalLoginsDTO?> GetExternalLoginsAsync();

    Task<bool> RemoveLoginAsync(RemoveLoginDTO dto);

    Task<RecoveryCodesStatusDTO?> GetRecoveryCodesStatusAsync();

    Task<RecoveryCodesWarningDTO?> GetRecoveryCodesWarningAsync();
}
