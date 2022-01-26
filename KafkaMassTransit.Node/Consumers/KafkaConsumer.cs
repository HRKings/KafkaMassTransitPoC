using KafkaMassTransit.Data;
using KafkaMassTransit.Data.Messages;
using MassTransit;

namespace KafkaMassTransit.Consumers;

public class KafkaConsumer: IConsumer<KafkaMessage>
{
    public Task Consume(ConsumeContext<KafkaMessage> context)
    {
        // Log when the request and/or event was processed
        Console.WriteLine(
            $"[{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}] {context.Message.Text} - {context.Message.SentTimestamp}");

        // If the incoming data is marked as event only (only Kafka is handling the event) then return nothing
        if (context.Message.IsEventOnly)
            return Task.CompletedTask;

        // If the message is RPC (a request coming through RabbitMQ) respond with the timestamp
        return context.RespondAsync(new KafkaResponse
        {
            Text = $"Responded in {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        });
    }
}