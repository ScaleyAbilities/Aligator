using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Asp
{
    static class RabbitHelper
    {
        private static IConnection rabbitConnection;
        private static IModel rabbitChannel;

        private static string rabbitHost = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "localhost";
        private static string rabbitCommandQueue = "commands";
        public static string rabbitResponseQueue = "response";

        private static IBasicProperties rabbitProperties;

        private static HashSet<string> declaredQueues = new HashSet<string>();

        static RabbitHelper() 
        {
            // Ensure Rabbit Queue is set up
            var factory = new ConnectionFactory() { 
                HostName = rabbitHost,
                UserName = "scaley",
                Password = "abilities"
            };

            rabbitConnection = factory.CreateConnection();
            rabbitChannel = rabbitConnection.CreateModel();

            rabbitProperties = rabbitChannel.CreateBasicProperties();
            rabbitProperties.Persistent = true;

            var consumer = new EventingBasicConsumer(rabbitChannel);
            consumer.Received += (model, eventArgs) =>
            {
                JObject message = null;
                try
                {
                    message = JObject.Parse(Encoding.UTF8.GetString(eventArgs.Body));
                }
                catch (JsonReaderException ex)
                {
                    Console.Error.WriteLine($"Unable to parse response message into JSON: {ex.Message}");
                }

                if (message != null)
                {
                    // Find the pending task in Program and complete it so the response can be sent
                    var reference = message["ref"]?.ToString();
                    TaskCompletionSource<JObject> pending;
                    Program.PendingResponses.TryGetValue(reference, out pending);

                    if (pending != null)
                        pending.SetResult(message);
                }
            };

            // This will begin consuming messages asynchronously
            rabbitChannel.BasicConsume(
                queue: rabbitResponseQueue,
                autoAck: true,
                consumer: consumer
            );
        }

        public static void PushCommand(JObject properties, int instance)
        {
            var queueKey = $"{rabbitCommandQueue}.{instance}";

            if (!declaredQueues.Contains(queueKey)) {
                rabbitChannel.QueueDeclare(
                    queue: queueKey,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                declaredQueues.Add(queueKey);
            }

            rabbitChannel.BasicPublish(
                exchange: "",
                routingKey: queueKey,
                basicProperties: rabbitProperties,
                body: Encoding.UTF8.GetBytes(properties.ToString(Formatting.None))
            );
        }
    }
}