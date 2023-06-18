using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
                        //o.QueryDelay = TimeSpan.FromSeconds(1);
                        //o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                        o.UsePostgres();
                        o.UseBusOutbox();

                    });

                    x.SetKebabCaseEndpointNameFormatter();

                    x.SetInMemorySagaRepositoryProvider();

                    var entry = Assembly.GetEntryAssembly();

                    //x.AddConsumers(entry);
                    x.AddSagaStateMachines(entry);
                    x.AddSagas(entry);


                    x.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(connectionString, c => c.TransportType = ServiceBusTransportType.AmqpWebSockets);

                        cfg.Message<Hello>(x =>
                        {
                            x.SetEntityName("topico-novo");

                        });

                        // TODO Partition
                        cfg.Send<Hello>(x =>
                        {
                            x.UsePartitionKeyFormatter(p => p.Message.MyProperty);
                            //   x.UseCorrelationId()
                            x.UseSerializer("application/json");


                        });


                        // TODO Correlation
                        cfg.Publish<Hello>(p =>
                        {
                            p.UserMetadata = "MetaDadosSigiloso";

                        });

                        cfg.ConfigureEndpoints(context);

                    });

                });

                services.AddHostedService<Worker>();
            });


    }

    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBus _bus;

        public Worker(IBus bus, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);
        }

        protected  async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scoped = _serviceProvider.CreateScope();
            var i = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("ok");


                var p = scoped.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var b = scoped.ServiceProvider.GetRequiredService<IBus>();


                await p.Publish<Hello>(new { MyProperty = $" IAE {i++}" }, ctx =>
                {

                    ctx.ContentType = new System.Net.Mime.ContentType("application/json");
                    ctx.CorrelationId = Guid.NewGuid();
                    //ctx.SetRoutingKey("rota");

                }, stoppingToken);

                await b.Publish<Hello>(new { MyProperty = $" IAE {i++}" }, ctx =>
                {

                    ctx.ContentType = new System.Net.Mime.ContentType("application/json");
                    ctx.CorrelationId = Guid.NewGuid();
                    //ctx.SetRoutingKey("rota");


                }, stoppingToken);


                await Task.Delay(1000, stoppingToken);

            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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


    public class OutContext : DbContext
    {
        public OutContext(DbContextOptions<OutContext> options) : base(options)
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