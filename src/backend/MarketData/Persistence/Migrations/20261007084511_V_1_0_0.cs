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
                    FxRateStalenessTolerance = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Funds", x => x.Id);
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
        }
    }
}
