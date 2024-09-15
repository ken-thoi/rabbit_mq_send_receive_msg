// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Hello, World!");

// Define the RabbitMQ connection settings
var factory = new ConnectionFactory()
{
    HostName = "localhost", // Change this to your RabbitMQ server hostname or IP
    Port = 5672, // Default RabbitMQ port
    UserName = "rabbitmq_user", // Replace with your username
    Password = "rabbitmq_user"  // Replace with your password
};

// Establish a connection to RabbitMQ
using (IConnection connection = factory.CreateConnection())
using (IModel channel = connection.CreateModel())
{
    // Declare a queue (the consumer will consume messages from this queue)
    channel.QueueDeclare(queue: "weather_queue",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

    Console.WriteLine(" [*] Waiting for messages.");

    // Create an event-driven consumer
    var consumer = new EventingBasicConsumer(channel);

    // Event handler to handle incoming messages
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] Received {0}", message);
    };

    // Start consuming messages from the queue
    channel.BasicConsume(queue: "weather_queue",
                         autoAck: true,
                         consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();  // Keep the console open until the user presses Enter
}