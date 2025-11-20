using System.ComponentModel.DataAnnotations;

namespace Microsoft.eShopWeb.Web.SharedDTOs;

	public class UserRegistrationDTO
	{
        [Required]
		public required string Email { get; set; }
        
        [Required]
		public required string Password { get; set; }

	}

