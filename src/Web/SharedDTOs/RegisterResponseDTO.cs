namespace Microsoft.eShopWeb.Web.SharedDTOs;

using Azure.Core;
using System.ComponentModel.DataAnnotations;
    public class RegisterResponseDTO
    {

        public required string Token { get; set; }


        public required bool Success { get; set; }


        public required string Message { get; set; }


    }

