using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices {
    public class MessageBusClient : IMessageBusClient {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration) {
            _configuration = configuration;

            var factory = new ConnectionFactory() {
                // Getting values from appsettings config
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try {
                // Set up a connection to RabbitMQ instance
                // 1. Create channel for RabbitMQ instance
                // 2. Create an Exchange for RabbitMQ instance
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                // Exchange is in charge of routing messages to different queues using header attributes, bindings, and routing keys.
                // Fanout exchange broadcasts all the messages it receives to all the queues it knows, the routing key is ignored.
                _channel.ExchangeDeclare(exchange: _configuration["RabbitMQExchange"], ExchangeType.Fanout);
                // Trigger this method when connection closes to Event Bus
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                 Console.WriteLine("--> Connected to Message Bus");
            } catch(Exception ex) {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }



        // Publishing message to Bus
        private void SendMessage(string message) {
            var body = Encoding.UTF8.GetBytes(message);

            // Specify Exchange where message should be sent
            _channel.BasicPublish(
                exchange: _configuration["RabbitMQExchange"],
                routingKey: "", // Fanout exchange ignores routing keys
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"--> We have sent {message}");

        }

        // Clean up resources
        public void Dispose() {
            Console.WriteLine($"--> Message Bus Disposed");
            if(_channel.IsOpen){
                _channel.Close();
                _connection.Close();
            }
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto) {
            // Serialize DTO into a String before sending to Message Bus
            var message = JsonSerializer.Serialize(platformPublishedDto);

            // Check if we have connection to RabbitMQ
            if(_connection.IsOpen) {
                Console.WriteLine("--> RabbitMQ Connection opened, sending message..");
                SendMessage(message);
            } else {
                Console.WriteLine("--> RabbitMQ Connection closed, not sending..");

            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }

    }
}