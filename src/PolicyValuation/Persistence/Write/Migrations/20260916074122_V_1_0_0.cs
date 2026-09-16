using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.PolicyValuation.Persistence.Write.Migrations
{
    /// <inheritdoc />
    public partial class V_1_0_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PolicyValuation");

            migrationBuilder.CreateTable(
                name: "__RebusInbox",
                schema: "PolicyValuation",
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
                name: "RetroactiveChanges",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetroactiveChanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsSealed = table.Column<bool>(type: "bit", nullable: false),
                    LatestValuationEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WarningMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastHandledRetroactiveChangeId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Policies_RetroactiveChanges_LastHandledRetroactiveChangeId",
                        column: x => x.LastHandledRetroactiveChangeId,
                        principalSchema: "PolicyValuation",
                        principalTable: "RetroactiveChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ValuationEvents",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyValuationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalReservesInPolicyCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalReservesInEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValuationEvents_Policies_PolicyValuationId",
                        column: x => x.PolicyValuationId,
                        principalSchema: "PolicyValuation",
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValuationMovements",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValuationEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    AmountInFundCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountInPolicyCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountInEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RawAmountInFundCurrency = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    RawAmountInPolicyCurrency = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    RawAmountInEur = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    NavDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NavValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    NavValuationMode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FundToPolicyFxRateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FundToPolicyFxRateValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: true),
                    PolicyToFundFxRateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PolicyToFundFxRateValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: true),
                    PolicyToEurFxRateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PolicyToEurFxRateValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValuationMovements_ValuationEvents_ValuationEventId",
                        column: x => x.ValuationEventId,
                        principalSchema: "PolicyValuation",
                        principalTable: "ValuationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValuationReserves",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValuationEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    AmountInFundCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountInPolicyCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountInEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RawAmountInFundCurrency = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    RawAmountInPolicyCurrency = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    RawAmountInEur = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    NavDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NavValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    FundToPolicyFxRateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FundToPolicyFxRateValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: true),
                    PolicyToFundFxRateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PolicyToFundFxRateValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: true),
                    PolicyToEurFxRateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PolicyToEurFxRateValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationReserves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValuationReserves_ValuationEvents_ValuationEventId",
                        column: x => x.ValuationEventId,
                        principalSchema: "PolicyValuation",
                        principalTable: "ValuationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___RebusInbox_ProcessedAt",
                schema: "PolicyValuation",
                table: "__RebusInbox",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_LastHandledRetroactiveChangeId",
                schema: "PolicyValuation",
                table: "Policies",
                column: "LastHandledRetroactiveChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_LatestValuationEventId",
                schema: "PolicyValuation",
                table: "Policies",
                column: "LatestValuationEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationEvents_PolicyValuationId_Date_OperationId",
                schema: "PolicyValuation",
                table: "ValuationEvents",
                columns: new[] { "PolicyValuationId", "Date", "OperationId" },
                unique: true,
                filter: "[OperationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationMovements_ValuationEventId",
                schema: "PolicyValuation",
                table: "ValuationMovements",
                column: "ValuationEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationReserves_ValuationEventId",
                schema: "PolicyValuation",
                table: "ValuationReserves",
                column: "ValuationEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_ValuationEvents_LatestValuationEventId",
                schema: "PolicyValuation",
                table: "Policies",
                column: "LatestValuationEventId",
                principalSchema: "PolicyValuation",
                principalTable: "ValuationEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_RetroactiveChanges_LastHandledRetroactiveChangeId",
                schema: "PolicyValuation",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_ValuationEvents_LatestValuationEventId",
                schema: "PolicyValuation",
                table: "Policies");

            migrationBuilder.DropTable(
                name: "__RebusInbox",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "ValuationMovements",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "ValuationReserves",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "RetroactiveChanges",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "ValuationEvents",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "Policies",
                schema: "PolicyValuation");
        }
    }
}
