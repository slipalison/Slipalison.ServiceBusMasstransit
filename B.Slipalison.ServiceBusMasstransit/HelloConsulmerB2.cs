using MassTransit;

namespace B.Slipalison.ServiceBusMasstransit
{
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
}