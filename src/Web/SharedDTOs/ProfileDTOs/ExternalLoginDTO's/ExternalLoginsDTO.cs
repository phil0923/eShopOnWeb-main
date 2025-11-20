namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs.ExternalLoginDTO_s;

public class ExternalLoginsDTO
{
    public List<ExternalLoginInfoDTO> CurrentLogins { get; set; } = new();
    public List<ExternalLoginProviderDTO> OtherLogins { get; set; } = new();
    public bool CanRemove { get; set; }
}
