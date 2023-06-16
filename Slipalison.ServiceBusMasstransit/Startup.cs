using MassTransit;
using Microsoft.OpenApi.Models;

// ...
namespace Slipalison.ServiceBusMasstransit;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configuração do MassTransit
        services.AddMassTransit(config =>
        {
            config.UsingAzureServiceBus((context, cfg) =>
            {
                var azureServiceBusConfig = Configuration.GetSection("AzureServiceBus");
                var connectionString = azureServiceBusConfig["ConnectionString"];
                var queueName = azureServiceBusConfig["QueueName"];

                cfg.Host(connectionString, c=> c.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets);
                //cfg.ReceiveEndpoint(queueName!, ep =>
                //{
                //    ep.ConfigureConsumer<SubmitOrderConsumer>(context);
                //});

                cfg.Send<SubmitOrder>(s => s.UseSessionIdFormatter(c => c.Message.OrderId));

                cfg.Publish<SubmitOrder>(x =>
                {
                    x.EnablePartitioning = true;
                });
                cfg.ConfigureEndpoints(context);
            });

           // config.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
        });

       // services.AddMassTransitHostedService();

        // Registro do consumidor
        services.AddScoped<SubmitOrderConsumer>();

        // ...

        services.AddControllers();

        services.AddSwaggerGen(c =>
         {
             c.SwaggerDoc("v1", new OpenApiInfo { Title = "Base API", Version = "v1" });
             c.UseInlineDefinitionsForEnums();
         });
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {


        app.UseSwagger();
        app.UseSwaggerUI();
        app//.UseHttpsRedirection()
            .UseRouting()
            // .UseResponseCompression()
            .UseEndpoints(builder => { builder.MapControllers(); });
            //.UseAuthorization();
    }
}
