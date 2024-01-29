using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<StateContainer>();

// Add custom HTTP Handler and register HttpClient
builder.Services.AddScoped(services =>
{
  var stateContainer = services.GetRequiredService<StateContainer>();
  return new CustomHttpHandler(stateContainer);
});

builder.Services.AddScoped(services =>
{
  var customHttpHandler = services.GetRequiredService<CustomHttpHandler>();
  var httpClientHandler = new HttpClientHandler();
  customHttpHandler.InnerHandler = httpClientHandler;

  var httpClient = new HttpClient(customHttpHandler)
  {
    BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress)
  };

  return httpClient;
});

await builder.Build().RunAsync();
