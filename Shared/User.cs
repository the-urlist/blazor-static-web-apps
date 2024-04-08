using System.Collections.Generic;

namespace BlazorApp.Shared;

public class User
{

  public User(string userName, string identityProvider)
  {
    UserName = userName;
    IdentityProvider = identityProvider;
    IsAuthenticated = true;
  }

  public User()
  {

  }

  public bool IsAuthenticated { get; set; } = false;

  public string UserName { get; set; }

  public string IdentityProvider { get; set; }

  public List<LinkBundle> LinkBundles { get; set; } = new List<LinkBundle>();
}
