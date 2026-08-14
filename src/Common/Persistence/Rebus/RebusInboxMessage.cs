namespace PAS.Persistence.Rebus;

public class RebusInboxMessage {
    public Guid MessageId { get; set; }
    public string MessageType { get; set; } = null!;
    public DateTime ProcessedAt { get; set; }
}