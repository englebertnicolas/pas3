using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PAS.MarketData.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MarketDbContext))]
    [Migration("20261007084512_V_1_0_0_Seed")]
    public class V_1_0_0_Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add views to share with other services
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [MarketData].[CurrenciesView] AS
                SELECT [Id], [EnglishName], [Symbol], [Decimals], [FxRateStalenessTolerance]
                FROM [MarketData].[Currencies];
            ");
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [MarketData].[CurrencyFxRatesView] AS
                SELECT [Id], [CurrencyId], [Date], [RateToEur]
                FROM [MarketData].[CurrencyFxRates];
            ");
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [MarketData].[FundsView] AS
                SELECT [Id], [Type], [Status], [Name], [Isin], [CurrencyId], [ValuationPeriodicity], [UnitDecimals], [NavDecimals], [NavPricingLag], [NavStalenessTolerance]
                FROM [MarketData].[Funds];
            ");
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [MarketData].[FundNavsView] AS
                SELECT [Id], [FundId], [Date], [Value]
                FROM [MarketData].[FundNavs];
            ");

            // Add default currencies
            // Not working with HasData because Symbol is defined as a ComplexProperty,
            // so we use raw SQL to insert the default currencies
            // (see EF issue: https://github.com/dotnet/efcore/issues/31254)
            migrationBuilder.Sql(@"
                INSERT INTO [MarketData].[Currencies] (Id, EnglishName, Symbol, Decimals, FxRateStalenessTolerance) VALUES 
                ('EUR', 'Euro', '€', 2, 7),
                ('USD', 'US Dollar', '$', 2, 7),
                ('GBP', 'British Pound', '£', 2, 7),
                ('JPY', 'Japanese Yen', '¥', 0, 7),
                ('CHF', 'Swiss Franc', 'CHF', 2, 7);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop views
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[FundNavsView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[FundsView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[CurrencyFxRatesView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[CurrenciesView];");
        }
    }
}
