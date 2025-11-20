namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;

public class UserProfileDTO
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailConfirmed { get; set; }

}
