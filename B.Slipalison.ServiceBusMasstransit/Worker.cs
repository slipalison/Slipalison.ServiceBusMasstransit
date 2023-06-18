using MassTransit;

namespace B.Slipalison.ServiceBusMasstransit
{
    public class Worker : BackgroundService
    {
        private readonly IBus _bus;

        public Worker(IBus bus)
        {
            _bus = bus;
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