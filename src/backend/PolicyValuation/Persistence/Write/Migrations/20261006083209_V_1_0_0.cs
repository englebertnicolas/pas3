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
                name: "Movements",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    AmountInPolicyCurrency = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    AmountInEur = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reserves",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    AmountInPolicyCurrency = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    AmountInEur = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValuationEvents",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LedgerId = table.Column<long>(type: "bigint", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalReservesInPolicyCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalReservesInEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValuationLedgers",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsSealed = table.Column<bool>(type: "bit", nullable: false),
                    LatestEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WarningMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastHandledRetroactiveChangeId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValuationLedgers_RetroactiveChanges_LastHandledRetroactiveChangeId",
                        column: x => x.LastHandledRetroactiveChangeId,
                        principalSchema: "PolicyValuation",
                        principalTable: "RetroactiveChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValuationLedgers_ValuationEvents_LatestEventId",
                        column: x => x.LatestEventId,
                        principalSchema: "PolicyValuation",
                        principalTable: "ValuationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX___RebusInbox_ProcessedAt",
                schema: "PolicyValuation",
                table: "__RebusInbox",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Movements_EventId",
                schema: "PolicyValuation",
                table: "Movements",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserves_EventId",
                schema: "PolicyValuation",
                table: "Reserves",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationEvents_LedgerId_Date_OperationId",
                schema: "PolicyValuation",
                table: "ValuationEvents",
                columns: new[] { "LedgerId", "Date", "OperationId" },
                unique: true,
                filter: "[OperationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationLedgers_LastHandledRetroactiveChangeId",
                schema: "PolicyValuation",
                table: "ValuationLedgers",
                column: "LastHandledRetroactiveChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationLedgers_LatestEventId",
                schema: "PolicyValuation",
                table: "ValuationLedgers",
                column: "LatestEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_ValuationEvents_EventId",
                schema: "PolicyValuation",
                table: "Movements",
                column: "EventId",
                principalSchema: "PolicyValuation",
                principalTable: "ValuationEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserves_ValuationEvents_EventId",
                schema: "PolicyValuation",
                table: "Reserves",
                column: "EventId",
                principalSchema: "PolicyValuation",
                principalTable: "ValuationEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ValuationEvents_ValuationLedgers_LedgerId",
                schema: "PolicyValuation",
                table: "ValuationEvents",
                column: "LedgerId",
                principalSchema: "PolicyValuation",
                principalTable: "ValuationLedgers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ValuationLedgers_ValuationEvents_LatestEventId",
                schema: "PolicyValuation",
                table: "ValuationLedgers");

            migrationBuilder.DropTable(
                name: "__RebusInbox",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "Movements",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "Reserves",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "ValuationEvents",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "ValuationLedgers",
                schema: "PolicyValuation");

            migrationBuilder.DropTable(
                name: "RetroactiveChanges",
                schema: "PolicyValuation");
        }
    }
}
