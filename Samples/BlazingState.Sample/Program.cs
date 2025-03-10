using BlazingState;
using BlazingState.Sample;
using BlazingState.Sample.Data;
using BlazingState.WebAssembly;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazingState()
    .AddAutoState()
    .AddStateObserver<CompanyInfo>(new CompanyInfo { Name = "InitialName" })
    .AddStateObserver<Account?>();


// Build
var app = builder.Build();

app.UseAutoState();


// Run
await app.RunAsync();