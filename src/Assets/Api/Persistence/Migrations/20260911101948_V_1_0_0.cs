using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Assets.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V_1_0_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Asset");

            migrationBuilder.CreateTable(
                name: "__RebusInbox",
                schema: "Asset",
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
                schema: "Asset",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyPairs",
                schema: "Asset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseCurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    QuoteCurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyPairs", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_CurrencyPairs_Currencies_BaseCurrencyId",
                        column: x => x.BaseCurrencyId,
                        principalSchema: "Asset",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyPairs_Currencies_QuoteCurrencyId",
                        column: x => x.QuoteCurrencyId,
                        principalSchema: "Asset",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                schema: "Asset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ValuationPeriodicity = table.Column<int>(type: "int", nullable: false),
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
                        principalSchema: "Asset",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyExchangeRates",
                schema: "Asset",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyPairId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyExchangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyExchangeRates_CurrencyPairs_CurrencyPairId",
                        column: x => x.CurrencyPairId,
                        principalSchema: "Asset",
                        principalTable: "CurrencyPairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundNavs",
                schema: "Asset",
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
                        principalSchema: "Asset",
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___RebusInbox_ProcessedAt",
                schema: "Asset",
                table: "__RebusInbox",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRates_CurrencyPairId",
                schema: "Asset",
                table: "CurrencyExchangeRates",
                column: "CurrencyPairId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRates_Date",
                schema: "Asset",
                table: "CurrencyExchangeRates",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyPairs_BaseCurrencyId",
                schema: "Asset",
                table: "CurrencyPairs",
                column: "BaseCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyPairs_QuoteCurrencyId",
                schema: "Asset",
                table: "CurrencyPairs",
                column: "QuoteCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FundNavs_Date",
                schema: "Asset",
                table: "FundNavs",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundNavs_FundId",
                schema: "Asset",
                table: "FundNavs",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_CurrencyId",
                schema: "Asset",
                table: "Funds",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                schema: "Asset",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Status",
                schema: "Asset",
                table: "Funds",
                column: "Status");

            // Add views to share with other services
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [Asset].[CurrenciesView] AS
                SELECT [Id], [EnglishName], [Symbol]
                FROM [Asset].[Currencies];
            ");
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [Asset].[CurrencyPairsView] AS
                SELECT [Id], [BaseCurrencyId], [QuoteCurrencyId]
                FROM [Asset].[CurrencyPairs];
            ");
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [Asset].[CurrencyExchangeRatesView] AS
                SELECT [CurrencyPairId], [Date], [Value]
                FROM [Asset].[CurrencyExchangeRates];
            ");
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [Asset].[FundsView] AS
                SELECT [Id], [Type], [Status], [Name], [Isin], [CurrencyId], [ValuationPeriodicity], [UnitDecimals], [NavDecimals], [NavPricingLag], [NavStalenessTolerance]
                FROM [Asset].[Funds];
            ");
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [Asset].[FundNavsView] AS
                SELECT [FundId], [Date], [Value]
                FROM [Asset].[FundNavs];
            ");

            // Add default currencies
            // Not working with HasData because Symbol is defined as a ComplexProperty,
            // so we use raw SQL to insert the default currencies
            // (see EF issue: https://github.com/dotnet/efcore/issues/31254)
            migrationBuilder.Sql(@"
                INSERT INTO [Asset].[Currencies] (Id, EnglishName, Symbol) VALUES 
                ('EUR', 'Euro', '€'),
                ('USD', 'US Dollar', '$'),
                ('GBP', 'British Pound', '£'),
                ('JPY', 'Japanese Yen', '¥'),
                ('CHF', 'Swiss Franc', 'CHF');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__RebusInbox",
                schema: "Asset");

            migrationBuilder.DropTable(
                name: "CurrencyExchangeRates",
                schema: "Asset");

            migrationBuilder.DropTable(
                name: "FundNavs",
                schema: "Asset");

            migrationBuilder.DropTable(
                name: "CurrencyPairs",
                schema: "Asset");

            migrationBuilder.DropTable(
                name: "Funds",
                schema: "Asset");

            migrationBuilder.DropTable(
                name: "Currencies",
                schema: "Asset");

            // Drop views
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [Asset].[FundNavsView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [Asset].[FundsView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [Asset].[CurrencyExchangeRatesView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [Asset].[CurrencyPairsView];");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [Asset].[CurrenciesView];");
        }
    }
}
