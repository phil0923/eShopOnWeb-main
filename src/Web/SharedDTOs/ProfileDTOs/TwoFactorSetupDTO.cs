namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;

public class TwoFactorSetupDTO
{
    public string SharedKey { get; set; } = null!;
    public string AuthenticatorUri { get; set; } = null!;
}

