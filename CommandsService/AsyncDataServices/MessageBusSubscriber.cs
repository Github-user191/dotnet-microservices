
using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AsyncDataServices {
    public class MessageBusSubscriber : BackgroundService {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor) {
            _configuration = configuration;
            _eventProcessor = eventProcessor;

            InitializeRabbitMQ();
        }

        // Creating our connection to RabbitMQ
        private void InitializeRabbitMQ() {
            var factory = new ConnectionFactory() {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

    
            try {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _configuration["RabbitMQExchange"], type: ExchangeType.Fanout);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: _queueName,
                exchange: _configuration["RabbitMQExchange"],
                routingKey: "");
                 Console.WriteLine("--> Listening on the Message Bus..");

                 _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            } catch(Exception ex) {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        // Clean up resources
        public void Dispose() {
            Console.WriteLine($"--> Message Bus Disposed");
            if(_channel.IsOpen){
                _channel.Close();
                _connection.Close();
            }
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ModuleHandle, ea) => {
                Console.WriteLine("--> Event Received!");
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                
                // ProcessEvent message determines the EventType and does the actions as specified in the Event Processor
                _eventProcessor.ProcessEvent(message);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }
    }
}