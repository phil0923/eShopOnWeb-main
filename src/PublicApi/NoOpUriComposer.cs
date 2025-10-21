using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.PublicApi;

public sealed class NoOpUriComposer : IUriComposer
{
    public string ComposePicUri(string uri) => uri ?? string.Empty;
}
