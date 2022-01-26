namespace KafkaMassTransit.Data.Messages;

public class KafkaMessage
{
    public string Text { get; set; }  = string.Empty;
    public long SentTimestamp { get; init; }
    public bool IsEventOnly { get; set; }
}