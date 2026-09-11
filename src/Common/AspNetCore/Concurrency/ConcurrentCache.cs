//using System.Collections.Concurrent;
//using PAS.OperationResults;

//namespace PAS.AspNetCore.Concurrency;

//public class ConcurrentCache<TKey, TValue> where TKey : notnull {
//    private readonly ConcurrentDictionary<TKey, TValue> cache = new();
//    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> locks = new();

//    public async Task<TValue> GetOrCreateAsync(TKey key, Func<Task<TValue>> factory) {
//        if (cache.TryGetValue(key, out var cachedValue))
//            return cachedValue;

//        var semaphore = locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
//        await semaphore.WaitAsync();
//        try {
//            if (cache.TryGetValue(key, out cachedValue))
//                return cachedValue;

//            var newValue = await factory();
//            cache[key] = newValue;
//            return newValue;

//        } finally {
//            semaphore.Release();
//        }
//    }

//    public async Task<bool> TryUpdateIfAsync(TKey key, Func<TValue, bool> condition, Func<TValue, Task<TValue>> factory) {
//        var semaphore = locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
//        await semaphore.WaitAsync();
//        try {
//            if (!cache.TryGetValue(key, out var currenValue))
//                return false;

//            if (condition(currenValue)) {
//                cache[key] = await factory(currenValue);
//                return true;
//            }
//            return false;

//        } finally {
//            semaphore.Release();
//        }
//    }

//    public async Task<ErrorOr<TValue>> GetOrCreateAsync(TKey key, Func<Task<ErrorOr<TValue>>> factory) {
//        if (cache.TryGetValue(key, out var cachedValue))
//            return cachedValue;

//        var semaphore = locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
//        await semaphore.WaitAsync();
//        try {
//            if (cache.TryGetValue(key, out cachedValue))
//                return cachedValue;

//            var newValue = await factory();
//            if (newValue.IsFailure)
//                return newValue.Errors;

//            cache[key] = newValue.Value;
//            return newValue;

//        } finally {
//            semaphore.Release();
//        }
//    }

//    public async Task<ErrorOr<bool>> TryUpdateIfAsync(TKey key, Func<TValue, bool> condition, Func<TValue, Task<ErrorOr<TValue>>> factory) {
//        var semaphore = locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
//        await semaphore.WaitAsync();
//        try {
//            if (!cache.TryGetValue(key, out var currenValue))
//                return false;

//            if (condition(currenValue)) {
//                var newValue = await factory(currenValue);
//                if (newValue.IsFailure)
//                    return newValue.Errors;

//                cache[key] = newValue.Value;
//                return true;
//            }
//            return false;

//        } finally {
//            semaphore.Release();
//        }
//    }
//}
