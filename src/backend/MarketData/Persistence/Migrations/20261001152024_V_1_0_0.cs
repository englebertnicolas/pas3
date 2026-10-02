using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.MarketData.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V_1_0_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MarketData");

            migrationBuilder.CreateTable(
                name: "__RebusInbox",
                schema: "MarketData",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___RebusInbox", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                schema: "MarketData",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Decimals = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyFxRates",
                schema: "MarketData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    RateToEur = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyFxRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyFxRates_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "MarketData",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                schema: "MarketData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ValuationPeriodicity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UnitDecimals = table.Column<int>(type: "int", nullable: false),
                    NavDecimals = table.Column<int>(type: "int", nullable: false),
                    NavPricingLag = table.Column<int>(type: "int", nullable: false),
                    NavStalenessTolerance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Funds_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "MarketData",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundNavs",
                schema: "MarketData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundNavs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundNavs_Funds_FundId",
                        column: x => x.FundId,
                        principalSchema: "MarketData",
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___RebusInbox_ProcessedAt",
                schema: "MarketData",
                table: "__RebusInbox",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyFxRates_CurrencyId_Date",
                schema: "MarketData",
                table: "CurrencyFxRates",
                columns: new[] { "CurrencyId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundNavs_Date",
                schema: "MarketData",
                table: "FundNavs",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundNavs_FundId",
                schema: "MarketData",
                table: "FundNavs",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_CurrencyId",
                schema: "MarketData",
                table: "Funds",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                schema: "MarketData",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Status",
                schema: "MarketData",
                table: "Funds",
                column: "Status");

            // Add views to share with other services
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [MarketData].[CurrenciesView] AS
                SELECT [Id], [EnglishName], [Symbol], [Decimals]
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
                INSERT INTO [MarketData].[Currencies] (Id, EnglishName, Symbol, Decimals) VALUES 
                ('EUR', 'Euro', '€', 2),
                ('USD', 'US Dollar', '$', 2),
                ('GBP', 'British Pound', '£', 2),
                ('JPY', 'Japanese Yen', '¥', 0),
                ('CHF', 'Swiss Franc', 'CHF', 2);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__RebusInbox",
                schema: "MarketData");

            migrationBuilder.DropTable(
                name: "CurrencyFxRates",
                schema: "MarketData");

            migrationBuilder.DropTable(
                name: "FundNavs",
                schema: "MarketData");

            migrationBuilder.DropTable(
                name: "Funds",
                schema: "MarketData");

            migrationBuilder.DropTable(
                name: "Currencies",
                schema: "MarketData");

            // Drop views
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[FundNavsView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[FundsView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[CurrencyFxRatesView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [MarketData].[CurrenciesView];");
        }
    }
}
