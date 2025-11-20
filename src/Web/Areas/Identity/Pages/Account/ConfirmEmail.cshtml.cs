using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.SharedDTOs.ProfileDTOs;

namespace Microsoft.eShopWeb.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailModel : PageModel
{
    private readonly IdentityServiceCaller _identityServiceCaller;

    public ConfirmEmailModel(IdentityServiceCaller identityServiceCaller)
    {
        _identityServiceCaller = identityServiceCaller;
    }

    public async Task<IActionResult> OnGetAsync(string userId, string code)
    {

      bool isConfirmed=  await _identityServiceCaller.ConfirmEmailAsync(new ConfirmEmailDTO() { UserId = userId, Token = code });
        if (isConfirmed!=true)
        {
            throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
        }

        return Page();
    }
}
