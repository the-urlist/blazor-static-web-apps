// using System.Collections.Generic;
// using System.Text.Json.Serialization;

// namespace BlazorApp.Shared;

// public class ClientPrincipalClaim
// {
//   [JsonPropertyName("typ")]
//   public string Type { get; set; }
//   [JsonPropertyName("val")]
//   public string Value { get; set; }
// }

// public class ClientPrincipal
// {
//   [JsonPropertyName("provider_name")]
//   public string IdentityProvider { get; set; }
//   [JsonPropertyName("user_id")]
//   public string UserId { get; set; }
//   [JsonPropertyName("role_typ")]
//   public IEnumerable<ClientPrincipalClaim> Claims { get; set; }
// }