using System.ComponentModel.DataAnnotations;

namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;

public class EnableTwoFactorDTO
{
    [Required]
    public string VerificationCode { get; set; } = null!;
}
