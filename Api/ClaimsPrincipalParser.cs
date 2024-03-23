using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker.Http;
using BlazorApp.Shared;

namespace Api;

public static class ClaimsPrincipalParser
{
  private class ClientPrincipalClaim
  {
    [JsonPropertyName("typ")]
    public string Type { get; set; }
    [JsonPropertyName("val")]
    public string Value { get; set; }
  }

  private class ClientPrincipal
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

  public static ClaimsPrincipal Parse(HttpRequestData req)
  {
    var principal = new ClientPrincipal();

    if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
    {
      var data = header.FirstOrDefault();
      var decoded = Convert.FromBase64String(data);
      var json = Encoding.UTF8.GetString(decoded);

      ClientPrincipal clientPrincipal;

      // handle functions claims structure
      if (json.StartsWith("["))
      {
        clientPrincipal = ParseFunctionsClaims(json);
      }

      // handle swa claims structure
      else
      {
        clientPrincipal = ParseSwaClaims(json);
      }

      try
      {
        principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      }
      catch (JsonException)
      {
        // Log the exception
      }
    }

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

    return new ClaimsPrincipal(identity);
  }

  // parse functions claims structure
  private static ClientPrincipal ParseFunctionsClaims(string json)
  {
    return JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
  }

  // parse SWA claims structure
  private static ClientPrincipal ParseSwaClaims(string json)
  {
    var clientPrincipalWrapper = JsonSerializer.Deserialize<ClientPrincipalWrapper>(json);

    var principal = new ClientPrincipal
    {
      IdentityProvider = clientPrincipalWrapper.ClientPrincipal.IdentityProvider,
      NameClaimType = clientPrincipalWrapper.ClientPrincipal.UserDetails,
    };

    return principal;
  }
}