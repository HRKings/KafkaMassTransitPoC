using KafkaMassTransit.Data.Messages;
using MassTransit;

namespace KafkaMassTransit.Consumers;

public class KafkaConsumer: IConsumer<KafkaMessage>
{
    public Task Consume(ConsumeContext<KafkaMessage> context)
    {
        Console.WriteLine(
            $"[{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}] {context.Message.Text} - {context.Message.SentTimestamp}");

        if (context.Message.IsEventOnly)
            return Task.CompletedTask;

        return context.RespondAsync<KafkaResponse>(new
        {
            Text = $"Responded in {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        });
    }
}