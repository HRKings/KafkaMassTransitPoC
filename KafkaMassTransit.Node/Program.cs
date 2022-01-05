using KafkaMassTransit.Data.Messages;
using MassTransit;
using MassTransit.KafkaIntegration;

var services = new ServiceCollection();

services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));

    x.AddRider(rider =>
    {
        rider.AddProducer<KafkaMessage>("topic-name");

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
    do
    {
        var value = await Task.Run(() =>
        {
            Console.WriteLine($"Enter text to send (or quit to exit)");
            Console.Write("> ");
            return Console.ReadLine();
        });

        if("quit".Equals(value, StringComparison.OrdinalIgnoreCase))
            break;
        
        await producer.Produce(new KafkaMessage
        {
            Text = value ?? "NA",
            SentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }
    while (true);
}
finally
{
    await busControl.StopAsync();
}