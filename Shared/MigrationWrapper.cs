using System;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
  public class MigrationWrapper
  {
    [JsonPropertyName("migration")]
    public Migration Migration { get; set; }
    [JsonPropertyName("migrationSiteURL")]
    public string MigrationSiteURL { get; set; }
  }
}