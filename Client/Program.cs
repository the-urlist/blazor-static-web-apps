using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<StateContainer>();

builder.Services.AddScoped(services =>
{
  var stateContainer = services.GetRequiredService<StateContainer>();
  return new CustomHttpHandler(stateContainer);
});
builder.Services.AddScoped(services =>
{
  var customHttpHanlder = services.GetRequiredService<CustomHttpHandler>();
  var httpClientHandler = new HttpClientHandler();
  customHttpHanlder.InnerHandler = httpClientHandler;

  var httpClient = new HttpClient(customHttpHanlder)
  {
    BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress)
  };

  return httpClient;
});

await builder.Build().RunAsync();
