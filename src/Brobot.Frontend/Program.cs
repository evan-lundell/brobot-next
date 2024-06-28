using Blazored.Toast;
using Brobot.Frontend;
using Brobot.Frontend.Handlers;
using Brobot.Frontend.Providers;
using Brobot.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var appUri = new Uri(builder.HostEnvironment.BaseAddress);
if (appUri == null)
{
    throw new Exception("Unable to get base uri");
}

builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<JwtAuthenticationStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(provider => provider.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped(provider => new JwtTokenMessageHandler(appUri, provider.GetRequiredService<JwtAuthenticationStateProvider>()));
builder.Services.AddHttpClient<ApiService>(client => client.BaseAddress = appUri)
    .AddHttpMessageHandler<JwtTokenMessageHandler>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddLogging();
builder.Services.AddBlazoredToast();

var app = builder.Build();
await app.Services.GetRequiredService<JwtService>().RefreshJwtToken();
await app.RunAsync();
