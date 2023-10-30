using System.Threading.Tasks;

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
}