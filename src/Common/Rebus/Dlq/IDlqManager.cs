namespace PAS.Rebus.Dlq;

public interface IDlqManager {
    Task<long> Count(CancellationToken cancellationToken = default);
    Task<DlqMessage[]> GetAsync(int topCount, CancellationToken cancellationToken = default);
    Task<int> ReplayAsync(int topCount, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int topCount, CancellationToken cancellationToken = default);
}
