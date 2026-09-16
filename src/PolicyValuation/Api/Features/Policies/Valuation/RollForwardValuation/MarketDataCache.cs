using AsyncKeyedLock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Persistence.Read;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public class MarketDataCache(ValuationReadDbContext dbContext, IMemoryCache memoryCache, AsyncKeyedLocker<string> lockManager) {
    private readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    #region Currencies

    public static string CurrenciesKey() => $"PolicyValuation-Currencies";

    public async Task<ErrorOr<CurrencyInfo>> GetCurrencyAsync(CurrencyId id, bool writeToCache = true, CancellationToken cancellationToken = default) {
        var eoCurrencies = await GetCurrenciesAsync(writeToCache, cancellationToken);
        if (eoCurrencies.IsFailure) return eoCurrencies.Errors;
        var currencies = eoCurrencies.Value;

        var item = currencies.FirstOrDefault(x => x.Id == id);
        if (item == null) return ErrorInfo.NotFound($"Currency '{id}' not found.");
        return item;
    }

    public async Task<ErrorOr<CurrencyInfo[]>> GetCurrenciesAsync(bool writeToCache = true, CancellationToken cancellationToken = default) {
        var key = CurrenciesKey();

        if (memoryCache.TryGetValue(key, out CurrencyInfo[]? cachedValue) && cachedValue != null) {
            return cachedValue;
        }

        using (await lockManager.LockAsync($"Currencies", cancellationToken)) {
            // A previous thread could have change the cache during the lock
            if (memoryCache.TryGetValue(key, out cachedValue) && cachedValue != null) {
                return cachedValue;
            }

            var eoCurrencyInfos = await LoadCurrenciesAsync(cancellationToken);
            if (eoCurrencyInfos.IsFailure) return eoCurrencyInfos.Errors;

            var newValues = eoCurrencyInfos.Value;
            if (writeToCache) {
                memoryCache.Set(key, newValues, new MemoryCacheEntryOptions {
                    SlidingExpiration = CacheExpiration
                });
            }

            return newValues;
        }
    }

    private async Task<ErrorOr<CurrencyInfo[]>> LoadCurrenciesAsync(CancellationToken cancellationToken) {
        return await dbContext.Currencies.AsNoTracking()
            .Select(x => new CurrencyInfo(new(x.Id), x.Decimals))
            .ToArrayAsync(cancellationToken);
    }

    #endregion

    #region Currency pairs

    public static string CurrencyPairKey(CurrencyId from, CurrencyId to) => $"PolicyValuation-CurrencyPair-{from}-{to}";

    public async Task<ErrorOr<CurrencyPairInfo>> GetCurrencyPairAsync(CurrencyId baseCurrencyId, CurrencyId quoteCurrencyId, DateOnly ratesSince, bool writeToCache = true, CancellationToken cancellationToken = default) {
        var key = CurrencyPairKey(baseCurrencyId, quoteCurrencyId);

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
            .Where(x => x.BaseCurrencyId == (string)baseCurrencyId && x.QuoteCurrencyId == (string)quoteCurrencyId)
            .Select(x => new CurrencyPairInfo(new(x.Id), new(x.BaseCurrencyId), new(x.QuoteCurrencyId), Array.Empty<CurrencyRateInfo>()))
            .SingleOrDefaultAsync(cancellationToken);

        if (currencyPairInfo == null)
            return ErrorInfo.NotFound($"Currency pair '{baseCurrencyId}-{quoteCurrencyId}' not found.");

        var minRateDate = ratesSince.AddDays(-7);
        var rates = await dbContext.CurrencyRates.AsNoTracking()
            .Where(x => x.CurrencyPairId == (Guid)currencyPairInfo.Id)
            .Where(x => x.Date >= minRateDate)
            .Select(x => new CurrencyRateInfo(x.Date, x.Value))
            .OrderBy(x => x.Date)
            .ToArrayAsync(cancellationToken);

        return currencyPairInfo with { CurrencyRates = rates };
    }

    private record CurrencyPairCacheItem(CurrencyPairInfo CurrencyPair, DateOnly RatesSince);

    #endregion

    #region Funds

    public static string FundKey(FundId id) => $"PolicyValuation-Fund-{id:N}";

    public async Task<ErrorOr<FundInfo>> GetFundAsync(FundId id, DateOnly navsSince, bool writeToCache = true, CancellationToken cancellationToken = default) {
        var key = FundKey(id);

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
            .Where(x => x.Id == (Guid)id)
            .Select(x => new FundInfo(new(x.Id), new(x.CurrencyId), Array.Empty<FundNavInfo>(), x.UnitDecimals, x.NavPricingLag, x.NavStalenessTolerance))
            .SingleOrDefaultAsync(cancellationToken);

        if (fundInfo == null)
            return ErrorInfo.NotFound($"Fund '{id}' not found.");

        var minNavDate = navsSince.AddDays(-fundInfo.NavStalenessTolerance);
        var navs = await dbContext.FundNavs.AsNoTracking()
            .Where(x => x.FundId == (Guid)id)
            .Where(x => x.Date >= minNavDate)
            .OrderBy(x => x.Date)
            .Select(x => new FundNavInfo(x.Date, x.Value))
            .ToArrayAsync(cancellationToken);

        return fundInfo with { Navs = navs };
    }

    private record FundCacheItem(FundInfo Fund, DateOnly NavsSince);

    #endregion
}
