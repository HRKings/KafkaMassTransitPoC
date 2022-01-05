using KafkaMassTransit.Data.Messages;
using MassTransit;
using MassTransit.KafkaIntegration;

var services = new ServiceCollection();

services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("amqp://rabbit:password@localhost:5672");
    });

    x.AddRider(rider =>
    {
        rider.AddProducer<KafkaMessage>("topic-name");
        rider.AddRequestClient<KafkaMessage>();

        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");
        });
    });
});

var provider = services.BuildServiceProvider();

var busControl = provider.GetRequiredService<IBusControl>();

await busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
try
{
    var producer = provider.GetRequiredService<ITopicProducer<KafkaMessage>>();
    var request = provider.GetRequiredService<IRequestClient<KafkaMessage>>();
    do
    {
        var value = await Task.Run(() =>
        {
            Console.WriteLine("Enter text to send (or quit to exit)");
            Console.Write("> ");
            return Console.ReadLine();
        });

        if("quit".Equals(value, StringComparison.OrdinalIgnoreCase))
            break;

        var message = new KafkaMessage
        {
            Text = "NA",
            SentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        message.Text = $"(Producer) {value}";
        await producer.Produce(message);

        message.Text = $"(Request) {value}";
        var response = await request.GetResponse<KafkaResponse>(message);
        
        Console.WriteLine(response.Message.Text);
    }
    while (true);
}
finally
{
    await busControl.StopAsync();
}