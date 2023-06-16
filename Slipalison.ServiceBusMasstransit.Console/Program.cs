using MassTransit;
using System;

namespace Console2.Slipalison.ServiceBusMasstransit
{
    public class Program
    {
        public static async Task Main()
        {
            // Configurar as informações de conexão com o Azure Service Bus
            var connectionString = "Endpoint=sb://slipalison.servicebus.windows.net/;SharedAccessKeyName=acesso;SharedAccessKey=gxX0D4bWpxXYl/nCKuoOVQDQEDg+peXf7+ASbCvZeOw=";
            var queueName = "submitorder-queue";

            // Configurar e criar o barramento MassTransit
            var bus = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                cfg.Host(connectionString, h =>
                {
                    h.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets;
                });

                cfg.Message<YourMessage>(x => x.SetEntityName(queueName));
            });

            // Iniciar o barramento
            await bus.StartAsync();

            try
            {
                // Loop para enviar várias mensagens
                while (true)
                {
                    Console.WriteLine("Digite uma mensagem (ou 'sair' para sair):");
                    var input = Console.ReadLine();

                    if (input.ToLower() == "sair")
                        break;

                    var message = new YourMessage { Text = input };

                    // Enviar a mensagem para a fila
                    await bus.Publish(message);

                    Console.WriteLine("Mensagem enviada com sucesso!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            finally
            {
                // Parar o barramento ao finalizar
                await bus.StopAsync();
            }
        }
    }

    public class YourMessage
    {
        public string Text { get; set; }
    }
}
