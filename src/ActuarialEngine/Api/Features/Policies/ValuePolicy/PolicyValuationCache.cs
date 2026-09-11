using AsyncKeyedLock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.Services.Models;
using PAS.ActuarialEngine.Persistence;

namespace PAS.ActuarialEngine.Features.Policies.ValuePolicy;

public class PolicyValuationCache(AssetReadOnlyDbContext dbContext, IMemoryCache memoryCache, AsyncKeyedLocker<string> lockManager) {
    private readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<ErrorOr<FundInfo>> GetFundAsync(FundId id, DateOnly navsSince, bool writeToCache = true, CancellationToken cancellationToken = default) {
        var key = $"PolicyValuation-Fund-{id.Value:N}";

        if (memoryCache.TryGetValue(key, out FundCacheItem? cachedItem) && cachedItem != null) {
            if (cachedItem.NavsSince <= navsSince) return cachedItem.Fund;
        }

        using (await lockManager.LockAsync(id.ToString(), cancellationToken)) {
            // A previous thread could have change the cache during the lock
            if (memoryCache.TryGetValue(key, out cachedItem) && cachedItem != null) {
                if (cachedItem.NavsSince <= navsSince) return cachedItem.Fund;
            }

            var eoFundInfo = await LoadFundAsync(id, navsSince, cancellationToken);
            if (eoFundInfo.IsFailure) return eoFundInfo.Errors;

            var newItem = new FundCacheItem(eoFundInfo.Value, navsSince);
            if (writeToCache) {
                memoryCache.Set(key, newItem, new MemoryCacheEntryOptions {
                    SlidingExpiration = CacheExpiration
                });
            }

            return newItem.Fund;
        }
    }

    private async Task<ErrorOr<FundInfo>> LoadFundAsync(FundId id, DateOnly navsSince, CancellationToken cancellationToken) {
        var fundInfo = await dbContext.Funds.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FundInfo(x.Id, x.CurrencyId, Array.Empty<FundNavInfo>(), x.UnitDecimals, x.NavPricingLag, x.NavStalenessTolerance))
            .SingleOrDefaultAsync(cancellationToken);

        if (fundInfo == null)
            return ErrorInfo.NotFound($"Fund '{id}' not found.");

        var minNavDate = navsSince.AddDays(-fundInfo.NavStalenessTolerance);
        var navs = await dbContext.FundNavs.AsNoTracking()
            .Where(x => x.FundId == id)
            .Where(x => x.Date >= minNavDate)
            .OrderBy(x => x.Date)
            .Select(x => new FundNavInfo(x.Date, x.Value))
            .ToArrayAsync(cancellationToken);

        return fundInfo with { Navs = navs };
    }

    private record FundCacheItem(FundInfo Fund, DateOnly NavsSince);

    public async Task<ErrorOr<CurrencyPairInfo>> GetCurrencyPairAsync(CurrencyId baseCurrencyId, CurrencyId quoteCurrencyId, DateOnly ratesSince, bool writeToCache = true, CancellationToken cancellationToken = default) {
        var key = $"PolicyValuation-CurrencyPair-{baseCurrencyId}-{quoteCurrencyId}";

        if (memoryCache.TryGetValue(key, out CurrencyPairCacheItem? cachedItem) && cachedItem != null) {
            if (cachedItem.RatesSince <= ratesSince) return cachedItem.CurrencyPair;
        }

        using (await lockManager.LockAsync($"{baseCurrencyId}>{quoteCurrencyId}", cancellationToken)) {
            // A previous thread could have change the cache during the lock
            if (memoryCache.TryGetValue(key, out cachedItem) && cachedItem != null) {
                if (cachedItem.RatesSince <= ratesSince) return cachedItem.CurrencyPair;
            }

            var eoCurrencyInfo = await LoadCurrencyPairAsync(baseCurrencyId, quoteCurrencyId, ratesSince, cancellationToken);
            if (eoCurrencyInfo.IsFailure) return eoCurrencyInfo.Errors;

            var newItem = new CurrencyPairCacheItem(eoCurrencyInfo.Value, ratesSince);
            if (writeToCache) {
                memoryCache.Set(key, newItem, new MemoryCacheEntryOptions {
                    SlidingExpiration = CacheExpiration
                });
            }

            return newItem.CurrencyPair;
        }
    }

    private async Task<ErrorOr<CurrencyPairInfo>> LoadCurrencyPairAsync(CurrencyId baseCurrencyId, CurrencyId quoteCurrencyId, DateOnly ratesSince, CancellationToken cancellationToken) {
        var currencyPairInfo = await dbContext.CurrencyPairs.AsNoTracking()
            .Where(x => x.BaseCurrencyId == baseCurrencyId && x.QuoteCurrencyId == quoteCurrencyId)
            .Select(x => new CurrencyPairInfo(x.Id, x.BaseCurrencyId, x.QuoteCurrencyId, Array.Empty<FxRateInfo>()))
            .SingleOrDefaultAsync(cancellationToken);

        if (currencyPairInfo == null)
            return ErrorInfo.NotFound($"Currency pair '{baseCurrencyId}-{quoteCurrencyId}' not found.");

        var minRateDate = ratesSince.AddDays(-7);
        var rates = await dbContext.CurrencyExchangeRates.AsNoTracking()
            .Where(x => x.CurrencyPairId == currencyPairInfo.Id)
            .Where(x => x.Date >= minRateDate)
            .Select(x => new FxRateInfo(x.Date, x.Value))
            .OrderBy(x => x.Date)
            .ToArrayAsync(cancellationToken);

        return currencyPairInfo with { FxRates = rates };
    }

    private record CurrencyPairCacheItem(CurrencyPairInfo CurrencyPair, DateOnly RatesSince);
}
