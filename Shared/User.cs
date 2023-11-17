using System;

namespace BlazorApp.Shared;

public class User
{
  public bool IsLoggedIn => ClientPrincipal != null && !string.IsNullOrEmpty(ClientPrincipal.UserId);
  public ClientPrincipal ClientPrincipal { get; set; } = new ClientPrincipal();
}
