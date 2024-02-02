using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Api.Utility
{
    public static class ClientPrincipalUtility
    {
        public static ClientPrincipal GetClientPrincipal(HttpRequestData req)
        {
            ClientPrincipal principal = null;

            if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
            {
                var data = header.FirstOrDefault();
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return principal;
        }
    }
}
