//using System.Collections.Concurrent;

//namespace PAS.AspNetCore.Concurrency;

//public class KeyedLockManager<TKey> where TKey : notnull {
//    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> semaphores = new();

//    public async Task<IDisposable> LockAsync(TKey key, CancellationToken ct = default) {
//        var semaphore = semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
//        await semaphore.WaitAsync(ct);
//        return new Releaser(semaphore);
//    }

//    private class Releaser(SemaphoreSlim semaphore) : IDisposable {
//        public void Dispose() => semaphore.Release();
//    }
//}