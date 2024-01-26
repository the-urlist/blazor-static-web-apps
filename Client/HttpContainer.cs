using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

public class HttpContainerResponse<T>
{
  public HttpStatusCode StatusCode { get; set; }
  public T? Data { get; set; }

  public HttpContainerResponse(HttpStatusCode statusCode, T? data)
  {
    StatusCode = statusCode;
    Data = data;
  }

  public static async Task<HttpContainerResponse<T>> CreateAsync(HttpResponseMessage response)
  {
    T data = default!;

    if (response.Content != null)
    {
      var content = await response.Content.ReadAsStringAsync();
      if (!string.IsNullOrEmpty(content))
      {
        data = JsonSerializer.Deserialize<T>(content) ?? default!;
      }
    }

    return new HttpContainerResponse<T>(response.StatusCode, data);
  }
}

public class HttpContainer
{
  private readonly HttpClient _httpClient;

  private int _requestQueue;

  public int RequestQueue
  {
    get => _requestQueue;
    set
    {
      _requestQueue = value;
      NotifyStateChanged();
    }
  }

  public HttpContainer(System.Uri baseAddress)
  {
    _httpClient = new HttpClient
    {
      BaseAddress = baseAddress,
      Timeout = System.TimeSpan.FromSeconds(10)
    };
  }

  public async Task<HttpContainerResponse<T>> GetAsync<T>(string requestUri)
  {
    var httpResponseMessage = await Execute(() => _httpClient.GetAsync(requestUri));
    return await HttpContainerResponse<T>.CreateAsync(httpResponseMessage);
  }

  public async Task<HttpContainerResponse<T>> PostAsync<T>(string requestUri, object? content)
  {
    var httpResponseMessage = await Execute(() => _httpClient.PostAsJsonAsync(requestUri, content));
    return await HttpContainerResponse<T>.CreateAsync(httpResponseMessage);
  }

  public async Task<HttpContainerResponse<T>> PutAsync<T>(string requestUri, object? content)
  {
    var httpResponseMessage = await Execute(() => _httpClient.PutAsJsonAsync(requestUri, content));
    return await HttpContainerResponse<T>.CreateAsync(httpResponseMessage);
  }

  public async Task<HttpContainerResponse<T>> DeleteAsync<T>(string requestUri)
  {
    var httpResponseMessage = await Execute(() => _httpClient.DeleteAsync(requestUri));
    return await HttpContainerResponse<T>.CreateAsync(httpResponseMessage);
  }

  private async Task<T> Execute<T>(Func<Task<T>> action)
  {
    RequestQueue++;

    try
    {
      var result = await action();
      return result;
    }
    catch (Exception ex)
    {
      throw new Exception(ex.Message);
    }
    finally
    {
      RequestQueue--;
    }
  }

  public event Action? OnChange;

  private void NotifyStateChanged() => OnChange?.Invoke();
}