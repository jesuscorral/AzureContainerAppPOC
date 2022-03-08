using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace AzureContainerAppPOC.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly DaprClient _daprClient;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        DaprClient darpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _daprClient = darpClient ?? throw new ArgumentNullException(nameof(darpClient));
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var gRPCPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT");
        var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");

        var (forecast, etag) = await _daprClient
            .GetStateAndETagAsync<IEnumerable<WeatherForecast>>(storeName: "statestore", key: "forecast");

        if (forecast is null)
        {
            forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            await _daprClient.TrySaveStateAsync(
                storeName: "statestore",
                key: "forecastv1",
                value: forecast,
                etag: etag,
                metadata: new Dictionary<string, string>()
                {
                    { "ttlInSeconds", "30" }
                });
        }

        return forecast;
       

    }
}
