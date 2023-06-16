using Azure.Messaging.ServiceBus;
using MassTransit;
using System.Reflection;

namespace C.Slipalison.ServiceBusMasstransit
{

    public class Program
    {

        public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {

                var connectionString = "Endpoint=sb://testebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dM4zOESViqVH6k/mPvp8zWtGR8tGaK9BW+ASbMrRJCw=";
                var queueName = "submitorder-queue";

                services.AddMassTransit(x =>
                {


                    x.SetKebabCaseEndpointNameFormatter();

                    x.SetInMemorySagaRepositoryProvider();

                    var entry = Assembly.GetEntryAssembly();

                  //  x.AddConsumers(entry);
                    x.AddSagaStateMachines(entry);
                    x.AddSagas(entry);


                    x.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(connectionString, c => c.TransportType = ServiceBusTransportType.AmqpWebSockets);
                        // cfg.Host(new Uri("sb://slipalison.servicebus.windows.net"), new ServiceBusClient(connectionString), new ServiceBusAdministrationClient(connectionString));


                        cfg.ConfigureEndpoints(context);


                        cfg.Message<Hello>(x => x.SetEntityName("topico-novo"));

                    });

                });

                services.AddHostedService<Worker>();
            });


    }

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
                Console.WriteLine("ok");

                await _bus.Publish(new Hello(), stoppingToken);

           
                await Task.Delay(1000, stoppingToken);

            }
        }
    }

    public class Hello
    {

        public string MyProperty { get; set; } = "Iae";
    }

    public class HelloConsulmerA : IConsumer<Hello>
    {
        public Task Consume(ConsumeContext<Hello> context)
        {
            Console.WriteLine(context.Message.MyProperty + "HelloConsulmerA");
            return Task.CompletedTask;
        }
    }

}