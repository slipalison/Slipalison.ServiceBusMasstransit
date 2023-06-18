using MassTransit;
using MassTransit.Serialization;

namespace B.Slipalison.ServiceBusMasstransit
{
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

            endpointConfigurator.UseConsumeFilter(typeof(GenericMiddleware<>), _serviceProvider);
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



    public class GenericMiddleware<T> : IFilter<ConsumeContext<T>> where T : class
    {
        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            try
            {



                // Executar código antes de processar a mensagem
                using var _ = Serilog.Context.LogContext.PushProperty("CorrelationID", context.CorrelationId.ToString());



                // Chamar o próximo middleware ou consumidor
                await next.Send(context);

                // Executar código depois de processar a mensagem
            }
            catch (Exception ex)
            {
                // Lidar com exceções, se necessário
                throw;
            }
        }

        public void Probe(ProbeContext context)
        {
            // Implementação vazia, não é necessário neste exemplo
            context.CreateFilterScope("transaction");
        }


    }

}