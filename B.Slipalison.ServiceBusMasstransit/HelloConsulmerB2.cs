using C.Slipalison.ServiceBusMasstransit;
using MassTransit;

namespace B.Slipalison.ServiceBusMasstransit;

public class HelloConsulmerB2 : IConsumer<Hello>
{
    private ILogger<HelloConsulmerB2> _logger;

    public HelloConsulmerB2(ILogger<HelloConsulmerB2> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<Hello> context)
    {
        try
        {
            _logger.LogInformation(context.Message.MyProperty + " HelloConsulmerB2");
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