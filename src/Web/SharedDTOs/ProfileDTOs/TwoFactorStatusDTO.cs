namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;


public class TwoFactorStatusDTO
{
    public bool Is2faEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
}


