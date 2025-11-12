namespace Microsoft.eShopWeb.Web.SharedDTOs;

public class LoginResponseDTO
{
    public required string Token { get; set; } = string.Empty;

    public required bool Succeeded { get; set; }

    public required bool RequiresTwoFactor { get; set; }

    public required bool IsLockedOut { get; set; }
}
