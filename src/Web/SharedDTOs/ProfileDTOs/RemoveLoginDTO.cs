namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;

public class RemoveLoginDTO
{
    /// <summary>
    /// The name of the external login provider (e.g., "Google", "Microsoft", "Facebook").
    /// </summary>
    public string LoginProvider { get; set; } = default!;

    /// <summary>
    /// The provider-specific unique key (e.g., the user ID from Google or Facebook).
    /// </summary>
    public string ProviderKey { get; set; } = default!;
}
