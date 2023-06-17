using Azure.Messaging.ServiceBus;
//using C.Slipalison.ServiceBusMasstransit;
using MassTransit;
using System.Reflection;

namespace B.Slipalison.ServiceBusMasstransit
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


                //  services.AddScoped(typeof(IConsumer<>));

                services.AddMassTransit(x =>
                {


                    x.SetKebabCaseEndpointNameFormatter();

                    x.SetInMemorySagaRepositoryProvider();

                    var entry = Assembly.GetEntryAssembly();

                    x.AddConsumers(entry);
                    x.AddSagaStateMachines(entry);
                    x.AddSagas(entry);


                    x.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(connectionString, c => c.TransportType = ServiceBusTransportType.AmqpWebSockets);

                        cfg.ConfigureEndpoints(context);

                        //cfg.ReceiveEndpoint("fila-b2", x =>
                        //{
                        //    x.ConfigureConsumeTopology = false;

                        //    x.UseRawJsonDeserializer();
                        //    x.UseJsonSerializer();
                        //    x.Subscribe("topico-novo", "subscriptionName-fila-b2");


                        //});

                    });

                    // x.AddConsumer<HelloConsulmerB2, SubsDefination>();

                });

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
                // Console.WriteLine("ok");

                //  await _bus.Publish(new Hello() {  MyProperty = "Iaae B"}, stoppingToken);


                await Task.Delay(1000, stoppingToken);

            }
        }
    }



    public class HelloConsulmerB2 : IConsumer<Hello>
    {
        public Task Consume(ConsumeContext<Hello> context)
        {
            Console.WriteLine(context.Message.MyProperty + " HelloConsulmerB2");
            return Task.CompletedTask;
        }
    }

    public class SubsDefination : ConsumerDefinition<HelloConsulmerB2>
    {

        public SubsDefination()
        {

            EndpointName = "EndPointName-1";
            ConcurrentMessageLimit = 2000;

        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<HelloConsulmerB2> consumerConfigurator)
        {
            if (endpointConfigurator is IServiceBusReceiveEndpointConfigurator c)
            {
                c.Subscribe("topico-novo", "NomeSubscription", x =>
                {
                    x.MaxDeliveryCount = 2000;

                });

            }

            endpointConfigurator.UseRawJsonDeserializer(MassTransit.Serialization.RawSerializerOptions.All);
            endpointConfigurator.UseRawJsonSerializer(MassTransit.Serialization.RawSerializerOptions.All);

            endpointConfigurator.PrefetchCount = 2000;
            endpointConfigurator.ConfigureConsumeTopology = false;

            // TODO: Retry 
            endpointConfigurator.UseMessageRetry(retry => retry.Incremental(3, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3)));
            // TODO Circuit Break 
            // TODO Outbox EF
            // DLQ

        }
    }
}