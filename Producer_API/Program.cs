using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/weatherforecast/{forecast_name}", (HttpRequest request,
                    string forecast_name,
                    [FromHeader(Name = "X-CUSTOM-HEADER")] string customHeader) => 
{
    var _forecast_name = request.RouteValues["forecast_name"];
    var _customHeader = request.Headers["X-CUSTOM-HEADER"];

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    var result = forecast.FirstOrDefault();

    //Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it, Define the RabbitMQ connection settings
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
        // Declare a queue (if it doesn't already exist)
        channel.QueueDeclare(queue: "weather_queue",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

        // Create the message to be sent
        string message = JsonConvert.SerializeObject(result);
        var body = Encoding.UTF8.GetBytes(message);

        // Publish the message to the default exchange
        channel.BasicPublish(exchange: "weather_exchange",
                                routingKey: "weather_queue", // The name of the queue
                                basicProperties: null,
                                body: body);
    }

    return result != null ? $"{result.Summary} - {result.TemperatureF}" : "";
})
.WithName("GetWeatherForecastByName")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
