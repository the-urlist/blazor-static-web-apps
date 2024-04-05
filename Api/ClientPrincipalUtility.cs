using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Utility
{
    public static class ClientPrincipalUtility
    {

        public class ClientPrincipalClaim
        {
            [JsonPropertyName("typ")]
            public string Type { get; set; }
            [JsonPropertyName("val")]
            public string Value { get; set; }
        }

        public class ClientPrincipal
        {
            [JsonPropertyName("auth_typ")]
            public string IdentityProvider { get; set; }
            [JsonPropertyName("name_typ")]
            public string NameClaimType { get; set; }
            [JsonPropertyName("role_typ")]
            public string RoleClaimType { get; set; }
            [JsonPropertyName("claims")]
            public IEnumerable<ClientPrincipalClaim> Claims { get; set; }
        }

        // public static ClientPrincipal GetClientPrincipal(HttpRequestData req)
        // {
        //     ClientPrincipal principal = null;

        //     var isDevelopment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development";

        //     if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
        //     {
        //         if (isDevelopment)
        //         {
        //             principal = ParseSWAPrincipal(header);
        //         }
        //         else
        //         {
        //             principal = ParseFunctionsPrincipal(header);
        //         }
        //     }

        //     return principal;
        // }

        // private static FunctionsClientPrincipal ParseSWAPrincipal(IEnumerable<string> header)
        // {
        //     ClientPrincipal principal = null;
        //     var data = header.FirstOrDefault();
        //     var decoded = Convert.FromBase64String(data);
        //     var json = Encoding.UTF8.GetString(decoded);
        //     principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //     FunctionsClientPrincipal functionsClientPrincipal = new FunctionsClientPrincipal
        //     {
        //         IdentityProvider = principal.IdentityProvider,
        //         NameClaimType = principal.UserDetails,
        //         RoleClaimType = "roles",
        //         Claims = new List<FunctionsClientPrincipalClaim>
        //         {
        //             new FunctionsClientPrincipalClaim
        //             {
        //                 Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
        //                 Value = principal.UserId
        //             },
        //             new FunctionsClientPrincipalClaim
        //             {
        //                 Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        //                 Value = principal.UserDetails
        //             },
        //             new FunctionsClientPrincipalClaim
        //             {
        //                 Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        //                 Value = "anonymous"
        //             }
        //         }
        //     };

        //     return functionsClientPrincipal;
        // }

        private static ClientPrincipal Parse(IEnumerable<string> header)
        {

            var principal = new ClientPrincipal();


            var data = header.FirstOrDefault();
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);

            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            /** 
             *  At this point, the code can iterate through `principal.Claims` to
             *  check claims as part of validation. Alternatively, we can convert
             *  it into a standard object with which to perform those checks later
             *  in the request pipeline. That object can also be leveraged for 
             *  associating user data, etc. The rest of this function performs such
             *  a conversion to create a `ClaimsPrincipal` as might be used in 
             *  other .NET code.
             */

            var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);
            identity.AddClaims(principal.Claims.Select(c => new Claim(c.Type, c.Value)));

            var claimsPrincipal = new ClaimsPrincipal(identity);

            return principal;
        }
    }

}
