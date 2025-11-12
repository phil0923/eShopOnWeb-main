namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs.ExternalLoginDTO_s;

public class ExternalLoginInfoDTO
{
    public string LoginProvider { get; set; } = default!;
    public string ProviderKey { get; set; } = default!;
    public string? ProviderDisplayName { get; set; }
}
