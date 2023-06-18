using MassTransit;

namespace B.Slipalison.ServiceBusMasstransit
{
    public class Worker : BackgroundService
    {
        private readonly IBus _bus;
        private readonly ILogger<Worker> _logger;

        public Worker(IBus bus, ILogger<Worker> logger)
        {
            _bus = bus;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Console.WriteLine("ok");

                //  await _bus.Publish(new Hello() {  MyProperty = "Iaae B"}, stoppingToken);


                await Task.Delay(1000, stoppingToken);

            }
        }
    }
}