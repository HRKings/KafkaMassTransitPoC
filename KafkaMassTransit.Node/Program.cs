using KafkaMassTransit.Consumers;
using KafkaMassTransit.Data.Messages;
using MassTransit;
using MassTransit.RabbitMqTransport;

// Create a default host and initialize the dependency injection collection
var host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    // Add MassTransit as an injectable service
    services.AddMassTransit(x =>
    {
        // Add the consumer to our workspace
        x.AddConsumer<KafkaConsumer>();
        
        // Configure RabbitMQ as our broker
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("amqp://rabbit:password@localhost:5672");
        
            // Set RabbitMQ as RPC service (consume and respond to requests)
            cfg.ReceiveEndpoint("kafka-message", (IRabbitMqReceiveEndpointConfigurator ep) =>
            {
                ep.ConfigureConsumer<KafkaConsumer>(context);
            });
        });

        // Add Kafka to the workspace
        x.AddRider(rider =>
        {
            // Add the consumer to Kafka too
            rider.AddConsumer<KafkaConsumer>();

            // Configure kafka
            rider.UsingKafka((context, k) =>
            {
                k.Host("localhost:9092");

                // Enable Kafka to consume events
                k.TopicEndpoint<KafkaMessage>("topic-name", "consumer-group-name", e =>
                {
                    // Create the topic if needed
                    e.CreateIfMissing(t =>
                    {
                        t.NumPartitions = 1; //number of partitions
                        t.ReplicationFactor = 1; //number of replicas
                    });
                    
                    // Configure one consumer
                    e.ConfigureConsumer<KafkaConsumer>(context);
                });
            });
        });
    });

    // Add Mass Transit services to the Dependency Injection Container
    services.AddMassTransitHostedService();
}).Build();

await host.RunAsync();