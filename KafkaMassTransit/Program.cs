using KafkaMassTransit.Consumers;
using KafkaMassTransit.Data.Messages;
using MassTransit;

var host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddMassTransit(x =>
    {
        x.UsingInMemory((context,config) => config.ConfigureEndpoints(context));

        x.AddRider(rider =>
        {
            rider.AddConsumer<KafkaConsumer>();

            rider.UsingKafka((context, k) =>
            {
                k.Host("localhost:9092");

                k.TopicEndpoint<KafkaMessage>("topic-name", "consumer-group-name", e =>
                {
                    e.CreateIfMissing(t =>
                    {
                        t.NumPartitions = 2; //number of partitions
                        t.ReplicationFactor = 1; //number of replicas
                    });
                    
                    e.ConfigureConsumer<KafkaConsumer>(context);
                });
            });
        });
    });

    services.AddMassTransitHostedService();
}).Build();

await host.RunAsync();