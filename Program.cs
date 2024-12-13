using Microsoft.AspNetCore.Mvc;
using OpenFeature;
using OpenFeature.Contrib.Providers.Flagd;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFeatureClient>(x =>
{
    var flagdConfig = new FlagdConfigBuilder()
    .WithHost("localhost")
    .WithPort(8013)
    .WithResolverType(ResolverType.RPC)
    .Build();

    var flagdProvider = new FlagdProvider(flagdConfig);
    Api.Instance.SetProviderAsync(flagdProvider).GetAwaiter().GetResult();
    var client = Api.Instance.GetClient();
    return client;
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("flag", async ([FromServices] IFeatureClient featureClient) =>
{
    bool showWelcomeBanner = false;
    if (await featureClient.GetBooleanValueAsync("show-welcome-banner", false))
    {
        showWelcomeBanner = true; 
    }

    string color = await featureClient.GetStringValueAsync("background-color", "red");

    if (!showWelcomeBanner) return Results.Ok("show welcome banner is not shown");

    return Results.Ok($"{color} coloured show welcome banner is shown");

});

app.Run();

//docker run --rm -it --name flagd -p 8013:8013 -v C:\Users\kursad.koc\source\repos\FlagSample\wwwroot:/etc/flagd ghcr.io/open-feature/flagd:latest start --uri file:./etc/flagd/flags.json
