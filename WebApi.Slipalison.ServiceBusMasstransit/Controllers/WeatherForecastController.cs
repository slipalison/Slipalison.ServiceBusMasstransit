using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Slipalison.ServiceBusMasstransit.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"    };
        private readonly IBus _bus;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IPublishEndpoint publishEndpoint, IBus bus)
        {
            _bus = bus;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get(CancellationToken cancellationToken)
        {

            await _publishEndpoint.Publish<Hello>(new()
            {
                MyProperty = "Web Api in Tha house "

            }, cancellationToken);


            await _bus.Publish<Hello>(new()
            {
                MyProperty = "Web Api in Tha house "

            }, cancellationToken);


            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}