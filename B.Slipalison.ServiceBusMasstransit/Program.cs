using Azure.Messaging.ServiceBus;
using MassTransit;
using MassTransit.Serialization;
using Microsoft.EntityFrameworkCore;
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

                var connectionStringPost = "User ID=bitstamp;Password=HZJqcT4It3liQC4O1H1p3cGUwhlX453U;Host=dpg-chn0kvm4dad21k1d9vn0-a.oregon-postgres.render.com;Port=5432;Database=bitstamp;Pooling=true;";


                services.AddDbContext<OutContext>(x =>
                {
                   
                    x.UseNpgsql(connectionStringPost, options =>
                    {
                        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                        options.MigrationsHistoryTable($"__{nameof(OutContext)}");

                        options.EnableRetryOnFailure(5);
                        options.MinBatchSize(1);
                    });
                });

                services.AddMassTransit(x =>
                {

                    x.AddEntityFrameworkOutbox<OutContext>(o =>
                    {
                        o.QueryDelay = TimeSpan.FromSeconds(1);
                        o.UsePostgres();
                        o.UseBusOutbox();

                    });

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
                        cfg.AutoStart = true;

                    });

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
        public async Task Consume(ConsumeContext<Hello> context)
        {
            try
            {
                Console.WriteLine(context.Message.MyProperty + " HelloConsulmerB2");
              //   throw new Exception("quebrei");

                await context.ConsumeCompleted;
                await context.NotifyConsumed(TimeSpan.Zero, nameof(HelloConsulmerB2));
            }
            catch (Exception ex)
            {

                //await context.NotifyFaulted(context, TimeSpan.Zero, nameof(HelloConsulmerB2), ex);
                throw;
            }

        }
    }

    public class SubsDefination : ConsumerDefinition<HelloConsulmerB2>
    {
        private readonly IServiceProvider _serviceProvider;

        public SubsDefination(IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
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

            endpointConfigurator.UseRawJsonDeserializer(RawSerializerOptions.All);
            endpointConfigurator.UseRawJsonSerializer(RawSerializerOptions.All);

            endpointConfigurator.PrefetchCount = 2000;
            endpointConfigurator.ConfigureConsumeTopology = false;

            // TODO: Retry 
            endpointConfigurator.UseMessageRetry(retry => retry.Incremental(3, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3)));

            // TODO Circuit Break 
            endpointConfigurator.UseCircuitBreaker(cb =>
            {
                cb.TrackingPeriod = TimeSpan.FromMinutes(5);
                cb.TripThreshold = 15;
                cb.ActiveThreshold = 10;
                cb.ResetInterval = TimeSpan.FromMinutes(5);
            });

            // KillSwitch 
            endpointConfigurator.UseKillSwitch(options => options
                .SetActivationThreshold(11)
                .SetTripThreshold(0.15)
                .SetRestartTimeout(m: 1));

            // TODO Outbox EF
            endpointConfigurator.UseEntityFrameworkOutbox<OutContext>(_serviceProvider);

        }
    }


    public class OutContext : DbContext
    {
        public OutContext(DbContextOptions<OutContext> options) :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}