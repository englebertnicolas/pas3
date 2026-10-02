using AsyncKeyedLock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PAS.PolicyValuation.Domain;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Persistence.Read;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public class MarketDataCache(ValuationReadDbContext dbContext, IMemoryCache memoryCache, AsyncKeyedLocker<string> lockManager)
{
    private readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    #region Currencies

    public static string CurrencyKey(CurrencyId id) => $"PolicyValuation-Currency-{id}";

    public async Task<ErrorOr<CurrencyInfo>> GetCurrencyAsync(CurrencyId id, DateOnly fxRatesSince, bool writeToCache = true, CancellationToken cancellationToken = default)
    {
        var key = CurrencyKey(id);

        if (memoryCache.TryGetValue(key, out CurrencyCacheItem? cachedItem) && cachedItem != null)
            if (cachedItem.FxRatesSince <= fxRatesSince) return cachedItem.Currency;

        using (await lockManager.LockAsync(key, cancellationToken))
        {
            // A previous thread could have change the cache during the lock
            if (memoryCache.TryGetValue(key, out cachedItem) && cachedItem != null)
                if (cachedItem.FxRatesSince <= fxRatesSince) return cachedItem.Currency;

            var eoCurrencyInfo = await LoadCurrencyAsync(id, fxRatesSince, cancellationToken);
            if (eoCurrencyInfo.IsFailure) return eoCurrencyInfo.Errors;

            var newItem = new CurrencyCacheItem(eoCurrencyInfo.Value, fxRatesSince);
            if (writeToCache)
            {
                memoryCache.Set(key, newItem, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = CacheExpiration
                });
            }

            return newItem.Currency;
        }
    }

    private async Task<ErrorOr<CurrencyInfo>> LoadCurrencyAsync(CurrencyId id, DateOnly fxRatesSince, CancellationToken cancellationToken)
    {
        var currencyInfo = await dbContext.Currencies.AsNoTracking()
            .Where(x => x.Id == (string)id)
            .Select(x => new CurrencyInfo(new(x.Id), x.Decimals, Array.Empty<CurrencyFxRateInfo>()))
            .SingleOrDefaultAsync(cancellationToken);

        if (currencyInfo == null)
            return ErrorInfo.NotFound($"Currency '{id}' not found.");

        var minFxRateDate = fxRatesSince.AddDays(-7);
        var navs = await dbContext.CurrencyFxRates.AsNoTracking()
            .Where(x => x.CurrencyId == (string)id)
            .Where(x => x.Date >= minFxRateDate)
            .OrderBy(x => x.Date)
            .Select(x => new CurrencyFxRateInfo(x.Date, x.RateToEur))
            .ToArrayAsync(cancellationToken);

        return currencyInfo with { FxRates = navs };
    }

    private record CurrencyCacheItem(CurrencyInfo Currency, DateOnly FxRatesSince);

    #endregion

    #region Funds

    public static string FundKey(FundId id) => $"PolicyValuation-Fund-{id:N}";

    public async Task<ErrorOr<FundInfo>> GetFundAsync(FundId id, DateOnly navsSince, bool writeToCache = true, CancellationToken cancellationToken = default)
    {
        var key = FundKey(id);

        if (memoryCache.TryGetValue(key, out FundCacheItem? cachedItem) && cachedItem != null)
            if (cachedItem.NavsSince <= navsSince) return cachedItem.Fund;

        using (await lockManager.LockAsync(key, cancellationToken))
        {
            // A previous thread could have change the cache during the lock
            if (memoryCache.TryGetValue(key, out cachedItem) && cachedItem != null)
                if (cachedItem.NavsSince <= navsSince) return cachedItem.Fund;

            var eoFundInfo = await LoadFundAsync(id, navsSince, cancellationToken);
            if (eoFundInfo.IsFailure) return eoFundInfo.Errors;

            var newItem = new FundCacheItem(eoFundInfo.Value, navsSince);
            if (writeToCache)
            {
                memoryCache.Set(key, newItem, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = CacheExpiration
                });
            }

            return newItem.Fund;
        }
    }

    private async Task<ErrorOr<FundInfo>> LoadFundAsync(FundId id, DateOnly navsSince, CancellationToken cancellationToken)
    {
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
