using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages.Shared.Components.BasketComponent;

public class Basket : ViewComponent
{
    private readonly IBasketViewModelService _basketService;
    private readonly ILogger<Basket> _logger;    

    public Basket(IBasketViewModelService basketService,ILogger<Basket> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Handle missing user safely
        var userName = HttpContext.User.Identity?.Name;

        if (string.IsNullOrEmpty(userName))
        {
            _logger.LogDebug("No user logged in. Returning empty basket view.");
            return View(new BasketComponentViewModel
            {
                ItemsCount = 0
            });
        }

        var vm = new BasketComponentViewModel
        {
            ItemsCount = await CountTotalBasketItems()
        };
        return View(vm);
    }

    private async Task<int> CountTotalBasketItems()
    {
        if (HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var userName = HttpContext.User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogDebug("Authenticated user with no name. Returning 0 items.");
                return 0;
            }
           
            return await _basketService.CountTotalBasketItems(userName);
        }

        string? anonymousId = GetAnnonymousIdFromCookie();
        if (string.IsNullOrEmpty(anonymousId))
            return 0;

        return await _basketService.CountTotalBasketItems(anonymousId);
    }

    private string? GetAnnonymousIdFromCookie()
    {
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            var id = Request.Cookies[Constants.BASKET_COOKIENAME];

            if (Guid.TryParse(id, out var _))
            {
                return id;
            }
        }
        return null;
    }
}
