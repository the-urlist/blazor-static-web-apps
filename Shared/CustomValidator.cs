using System;

namespace BlazorApp.Shared;

public class CustomValidator
{
  public bool Invalid
  {
    get
    {
      return !Valid;
    }
  }
  public bool Valid { get; private set; } = true;
  public string Message { get; set; } = "";
  public string validationErrorClass
  {
    get
    {
      return Invalid ? "invalid" : "";
    }
  }

  public string ValidationMessageClass
  {
    get
    {
      return Valid ? "is-invisible" : "";
    }
  }

  public void Set(bool valid, string message)
  {
    Valid = valid;
    Message = message;
  }

  public void Reset()
  {
    Valid = true;
    Message = "";
  }

  public void ValidateVanityURLChars(string vanityUrl)
  {
    var regex = new System.Text.RegularExpressions.Regex(@"^(^$|[a-zA-Z0-9_\-])+$");

    if (!regex.IsMatch(vanityUrl))
    {
      Set(false, "Only letters, numbers, underscores, and dashes are allowed.");
    }
  }

  public void ValidateURL(string url)
  {
    var regex = new System.Text.RegularExpressions.Regex(@"^(https?://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$");

    if (!regex.IsMatch(url))
    {
      Set(false, "That doesn't look like a valid URL");
    }
  }
}