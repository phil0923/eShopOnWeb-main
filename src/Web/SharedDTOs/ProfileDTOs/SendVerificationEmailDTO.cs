using System.ComponentModel.DataAnnotations;

namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;

public class SendVerificationEmailDTO
{

    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}
