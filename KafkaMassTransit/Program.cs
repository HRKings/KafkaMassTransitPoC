using KafkaMassTransit.Consumers;
using KafkaMassTransit.Data.Messages;
using MassTransit;
using MassTransit.RabbitMqTransport;

var host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddMassTransit(x =>
    {
        x.AddConsumer<KafkaConsumer>();
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("amqp://rabbit:password@localhost:5672");
        
            cfg.ReceiveEndpoint("kafka-message", (IRabbitMqReceiveEndpointConfigurator ep) =>
            {
                ep.ConfigureConsumer<KafkaConsumer>(context);
            });
        });

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
                        t.NumPartitions = 1; //number of partitions
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