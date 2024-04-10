using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var environment = builder.HostEnvironment;

var API_Prefix = environment.IsDevelopment() ? builder.Configuration["DEV_API_Prefix"] : builder.Configuration["PROD_API_Prefix"];

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(API_Prefix ?? builder.HostEnvironment.BaseAddress) });
builder.Services.AddCascadingValue("API_Prefix", sp => API_Prefix ?? builder.HostEnvironment.BaseAddress);
builder.Services.AddSingleton<StateContainer>();

await builder.Build().RunAsync();
