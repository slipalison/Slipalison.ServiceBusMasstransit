using MassTransit;

namespace Slipalison.ServiceBusMasstransit;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var order = context.Message;

        // Lógica para processar a mensagem do pedido
        // ...

        await Console.Out.WriteLineAsync($"Received order: {order.OrderId} - {order.ProductName}");
    }
}