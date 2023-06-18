using Azure.Messaging.ServiceBus;
using MassTransit;
using System.Reflection;

namespace B.Slipalison.ServiceBusMasstransit
{
    public static class MasstransitExtensions {

        public static IServiceCollection AddAzBusMasstransit(this IServiceCollection services) {


            var connectionString = "Endpoint=sb://testebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dM4zOESViqVH6k/mPvp8zWtGR8tGaK9BW+ASbMrRJCw=";
            var queueName = "submitorder-queue";
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


            return services;
        }
    }
}