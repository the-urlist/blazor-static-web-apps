using System;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
  public class Migration
  {
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; set; }
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
  }
}