using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
public class CustomHttpClient : HttpClient
{
	private readonly IWebAssemblyHostEnvironment _env;
	public CustomHttpClient(HttpMessageHandler handler, IWebAssemblyHostEnvironment env) : base(handler)
	{
		_env = env;
	}

	public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Add the header to the request
		// if this is development, add the x-ms-client-principal header
		if (_env.IsDevelopment())
		{
			request.Headers.Add("xs-ms-client-principal",
				"eyJhdXRoX3R5cCI6InR3aXR0ZXIiLCJjbGFpbXMiOlt7InR5cCI6Imh0dHA6XC9cL3NjaGVtYXMueG1sc29hcC5vcmdcL3dzXC8yMDA1XC8wNVwvaWRlbnRpdHlcL2NsYWltc1wvbmFtZWlkZW50aWZpZXIiLCJ2YWwiOiIxNjUzNDM0MjUwMTc0MTExNzQ0In0seyJ0eXAiOiJodHRwOlwvXC9zY2hlbWFzLnhtbHNvYXAub3JnXC93c1wvMjAwNVwvMDVcL2lkZW50aXR5XC9jbGFpbXNcL3VwbiIsInZhbCI6InRoZV91cmxpc3QifSx7InR5cCI6Imh0dHA6XC9cL3NjaGVtYXMueG1sc29hcC5vcmdcL3dzXC8yMDA1XC8wNVwvaWRlbnRpdHlcL2NsYWltc1wvbmFtZSIsInZhbCI6InVybGlzdCJ9LHsidHlwIjoidXJuOnR3aXR0ZXI6ZGVzY3JpcHRpb24iLCJ2YWwiOiIifSx7InR5cCI6InVybjp0d2l0dGVyOmxvY2F0aW9uIiwidmFsIjoiIn0seyJ0eXAiOiJ1cm46dHdpdHRlcjp2ZXJpZmllZCIsInZhbCI6IkZhbHNlIn0seyJ0eXAiOiJ1cm46dHdpdHRlcjpwcm9maWxlX2ltYWdlX3VybF9odHRwcyIsInZhbCI6Imh0dHBzOlwvXC9hYnMudHdpbWcuY29tXC9zdGlja3lcL2RlZmF1bHRfcHJvZmlsZV9pbWFnZXNcL2RlZmF1bHRfcHJvZmlsZV9ub3JtYWwucG5nIn1dLCJuYW1lX3R5cCI6Imh0dHA6XC9cL3NjaGVtYXMueG1sc29hcC5vcmdcL3dzXC8yMDA1XC8wNVwvaWRlbnRpdHlcL2NsYWltc1wvdXBuIiwicm9sZV90eXAiOiJodHRwOlwvXC9zY2hlbWFzLm1pY3Jvc29mdC5jb21cL3dzXC8yMDA4XC8wNlwvaWRlbnRpdHlcL2NsYWltc1wvcm9sZSJ9");

			// Call the base SendAsync method to send the request
			return await base.SendAsync(request, cancellationToken);
		}
		else
		{
			// Call the base SendAsync method to send the request
			return await base.SendAsync(request, cancellationToken);
		}
	}
}