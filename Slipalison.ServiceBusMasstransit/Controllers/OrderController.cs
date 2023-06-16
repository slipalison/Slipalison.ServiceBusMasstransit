using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Slipalison.ServiceBusMasstransit.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private ServiceBusClient client;
        private ServiceBusSender sender;

        public OrderController(IPublishEndpoint publishEndpoint, IConfiguration configuration)
        {
            _publishEndpoint = publishEndpoint;
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };


            var azureServiceBusConfig = configuration.GetSection("AzureServiceBus");
            var connectionString = azureServiceBusConfig["ConnectionString"];
            var queueName = azureServiceBusConfig["QueueName"];

            client = new ServiceBusClient(connectionString, clientOptions);
            sender = client.CreateSender(queueName);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitOrder(CancellationToken cancellationToken)
        {
            try
            {



                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();


                for (int i = 1; i <= 4; i++)
                {
                    // try adding a message to the batch
                    if (!messageBatch.TryAddMessage(new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(new SubmitOrder()
                    {
                        OrderId = "opa",
                        ProductName = "novo"
                    }))))
                    {
                        // if it is too large for the batch
                        throw new Exception($"The message {i} is too large to fit in the batch.");
                    }
                }



                try
                {
                    // Use the producer client to send the batch of messages to the Service Bus queue
                    await sender.SendMessagesAsync(messageBatch);
                    Console.WriteLine($"A batch of {4} messages has been published to the queue.");
                }
                finally
                {
                    // Calling DisposeAsync on client types is required to ensure that network
                    // resources and other unmanaged objects are properly cleaned up.
                    await sender.DisposeAsync();
                    await client.DisposeAsync();
                }


                // Envio da mensagem para o Azure Service Bus
                await _publishEndpoint.Publish(new SubmitOrder()
                {
                    OrderId = "opa",
                    ProductName = "novo"
                }, cancellationToken);
            }
            catch (Exception ex)
            {

                throw;
            }
        

            return Ok();
        }
    }
}
