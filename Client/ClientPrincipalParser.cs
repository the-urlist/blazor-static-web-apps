using BlazorApp.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ClientPrincipalParser
{
  public class SWAClientPrincipalWrapper
  {
    [JsonPropertyName("clientPrincipal")]
    public SWAClientPrincipal? ClientPrincipal { get; set; }
  }

  public class SWAClientPrincipal
  {
    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; set; } = string.Empty;

    [JsonPropertyName("userDetails")]
    public string UserDetails { get; set; } = string.Empty;
  }

  public class FunctionsClientPrincipal
  {
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("provider_name")]
    public string IdentityProvider { get; set; } = string.Empty;
  }

  [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Shared assembly hasn't opted into trimming")]
  public static User Parse(string clientPrincipalJson, bool isDevelopment)
  {
    User user;

    if (isDevelopment)
    {

      var clientPrincipalWrapper = JsonSerializer.Deserialize<SWAClientPrincipalWrapper>(clientPrincipalJson);

      if (clientPrincipalWrapper?.ClientPrincipal == null)
      {
        // 401 is what Azure Functions returns when the user is not logged in. We do this to make the emulator behave the same way.
        throw new HttpRequestException("Unauthorized", null, System.Net.HttpStatusCode.Unauthorized);
      }

      user = new User(clientPrincipalWrapper?.ClientPrincipal.UserDetails, clientPrincipalWrapper?.ClientPrincipal.IdentityProvider);
    }
    else
    {
      var clientPrincipal = JsonSerializer.Deserialize<FunctionsClientPrincipal>(clientPrincipalJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      user = new User(clientPrincipal?.UserId, clientPrincipal?.IdentityProvider);
    };

    return user;
  }
}