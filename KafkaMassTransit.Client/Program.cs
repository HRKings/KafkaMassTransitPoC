using KafkaMassTransit.Data;
using KafkaMassTransit.Data.Messages;
using MassTransit;
using MassTransit.KafkaIntegration;

// Create the service collection used in dependency injection
var services = new ServiceCollection();

// Add MassTransit as an injectable service
services.AddMassTransit(x =>
{
    // Configure RabbitMQ as the broker for RPC
    x.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host("amqp://rabbit:password@localhost:5672");
    });

    // Add Kafka as a possible event source
    x.AddRider(rider =>
    {
        // Add a event producer
        rider.AddProducer<KafkaMessage>("topic-name");
        // Add a RPC client that can create an event and also receive a response via RabbitMQ
        rider.AddRequestClient<KafkaMessage>();

        // Configure the Kafka node to be used
        rider.UsingKafka((_, k) =>
        {
            k.Host("localhost:9092");
        });
    });
});

// Build the Dependency Injection Provider
var provider = services.BuildServiceProvider();

// Get the Bus Control service that MassTransit creates
var busControl = provider.GetRequiredService<IBusControl>();

// Start the Bus Control
await busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
try
{
    // Get both the event producer and the request client
    var eventProducer = provider.GetRequiredService<ITopicProducer<KafkaMessage>>();
    var requestClient = provider.GetRequiredService<IRequestClient<KafkaMessage>>();
    
    // Enter the main loop
    do
    {
        // Constantly get the user input in another thread
        var value = await Task.Run(() =>
        {
            Console.WriteLine("Enter text to send (or quit to exit)");
            Console.Write("> ");
            return Console.ReadLine();
        });
        
        // If nothing is entered, go back to the loop start
        if (string.IsNullOrWhiteSpace(value))
            continue;

        // Quit the loop if the user requests so
        if("quit".Equals(value, StringComparison.OrdinalIgnoreCase))
            break;

        // If any text is entered, create a new DTO for the message
        var message = new KafkaMessage
        {
            Text = "NA",
            SentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // Mark the the message as coming from the event producer and only triggers an event (Fire and Forget)
        message.Text = $"(Event) {value}";
        message.IsEventOnly = true;
        await eventProducer.Produce(message);

        // Mark the message as RPC and use the client to get a response back
        message.Text = $"(RPC) {value}";
        message.IsEventOnly = false;
        var response = await requestClient.GetResponse<KafkaResponse>(message);
        Console.WriteLine(response.Message.Text);
    }
    while (true);
}
finally
{
    await busControl.StopAsync();
}