namespace BlazorApp.Client
{
  public class CustomHttpHandler : DelegatingHandler
  {
    private readonly StateContainer _stateContainer;

    public CustomHttpHandler(StateContainer stateContainer)
    {
      _stateContainer = stateContainer;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      _stateContainer.ActiveHttpRequests++;
      try
      {
        return await base.SendAsync(request, cancellationToken);
      }
      catch (Exception)
      {
        throw;
      }
      finally
      {
        _stateContainer.ActiveHttpRequests--;
      }
    }
  }
}