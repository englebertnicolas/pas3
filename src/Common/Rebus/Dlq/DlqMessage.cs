namespace PAS.Rebus.Dlq;

public record DlqMessage {
    public required string Id { get; init; }
    public required string MessageId { get; init; }
    public required string MessageType { get; init; }
    public required string SenderAddress { get; init; }
    public required DateTime? SentTime { get; init; }
    public required string SourceQueue { get; init; }
    public required string ExceptionMessage { get; init; }
    public required string BodyPreview { get; init; }
    public required int ReplayCount { get; init; }
    public required DateTime? LastReplayTime { get; init; }
}
