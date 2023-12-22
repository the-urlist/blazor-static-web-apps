using System.Text.Json.Serialization;

namespace BlazorApp.Shared;

public class ClientPrincipalWrapper
{
  [JsonPropertyName("clientPrincipal")]
  public ClientPrincipal ClientPrincipal { get; set; } = new ClientPrincipal();
}
