using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker.Http;

namespace BlazorApp.Shared;

public class ClientPrincipalParser
{

  public class FunctionsClientPrincipal
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

  public class ClientPrincipalClaim
  {
    [JsonPropertyName("typ")]
    public string Type { get; set; }
    [JsonPropertyName("val")]
    public string Value { get; set; }
  }

  public class SWAClientPrincipal
  {
    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; set; } = string.Empty;

    [JsonPropertyName("userDetails")]
    public string UserDetails { get; set; } = string.Empty;
  }

  public static User Parse(HttpRequestData req)
  {
    var isDevelopment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development";
    User user = null;

    if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
    {
      var data = header.FirstOrDefault();
      var decoded = Convert.FromBase64String(data);
      var json = Encoding.UTF8.GetString(decoded);

      if (isDevelopment)
      {
        var clientPrincipal = JsonSerializer.Deserialize<SWAClientPrincipal>(json);

        user = new User(clientPrincipal?.UserDetails, clientPrincipal?.IdentityProvider);
      }
      else
      {

        var clientPrincipal = JsonSerializer.Deserialize<FunctionsClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var identity = new ClaimsIdentity(clientPrincipal.IdentityProvider, clientPrincipal.NameClaimType, clientPrincipal.RoleClaimType);
        identity.AddClaims(clientPrincipal.Claims.Select(c => new Claim(c.Type, c.Value)));

        var claimsPrincipal = new ClaimsPrincipal(identity);

        user = new User(claimsPrincipal.Identity.Name, claimsPrincipal.Identity.AuthenticationType);
      };
    }

    return user;
  }
}