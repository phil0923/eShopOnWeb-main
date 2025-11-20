using System.ComponentModel.DataAnnotations;

namespace Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;

public class SetPasswordDTO
{

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
