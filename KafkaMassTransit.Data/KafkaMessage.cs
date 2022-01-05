namespace KafkaMassTransit.Data.Messages;

public class KafkaMessage
{
    public string Text { get; set; }
    public long SentTimestamp { get; set; }
}