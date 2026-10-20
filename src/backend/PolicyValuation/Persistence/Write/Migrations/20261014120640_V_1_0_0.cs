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
                name: "ValuationLedgers",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsSealed = table.Column<bool>(type: "bit", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "CalculationLayers",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LedgerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    RetroactiveChangeId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationLayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculationLayers_RetroactiveChanges_RetroactiveChangeId",
                        column: x => x.RetroactiveChangeId,
                        principalSchema: "PolicyValuation",
                        principalTable: "RetroactiveChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalculationLayers_ValuationLedgers_LedgerId",
                        column: x => x.LedgerId,
                        principalSchema: "PolicyValuation",
                        principalTable: "ValuationLedgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValuationEvents",
                schema: "PolicyValuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LedgerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculationLayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolledbackByCalculationLayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalReservesInPolicyCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalReservesInEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValuationEvents_CalculationLayers_CalculationLayerId",
                        column: x => x.CalculationLayerId,
                        principalSchema: "PolicyValuation",
                        principalTable: "CalculationLayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValuationEvents_CalculationLayers_RolledbackByCalculationLayerId",
                        column: x => x.RolledbackByCalculationLayerId,
                        principalSchema: "PolicyValuation",
                        principalTable: "CalculationLayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValuationEvents_ValuationLedgers_LedgerId",
                        column: x => x.LedgerId,
                        principalSchema: "PolicyValuation",
                        principalTable: "ValuationLedgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_Movements_ValuationEvents_EventId",
                        column: x => x.EventId,
                        principalSchema: "PolicyValuation",
                        principalTable: "ValuationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_Reserves_ValuationEvents_EventId",
                        column: x => x.EventId,
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
                name: "IX_CalculationLayers_LedgerId",
                schema: "PolicyValuation",
                table: "CalculationLayers",
                column: "LedgerId",
                unique: true,
                filter: "[IsCurrent] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CalculationLayers_LedgerId_Seq",
                schema: "PolicyValuation",
                table: "CalculationLayers",
                columns: new[] { "LedgerId", "Seq" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculationLayers_RetroactiveChangeId",
                schema: "PolicyValuation",
                table: "CalculationLayers",
                column: "RetroactiveChangeId");

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
                name: "IX_ValuationEvents_CalculationLayerId",
                schema: "PolicyValuation",
                table: "ValuationEvents",
                column: "CalculationLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationEvents_LedgerId_CalculationLayerId_Date_OperationId",
                schema: "PolicyValuation",
                table: "ValuationEvents",
                columns: new[] { "LedgerId", "CalculationLayerId", "Date", "OperationId" },
                unique: true,
                filter: "[OperationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationEvents_RolledbackByCalculationLayerId",
                schema: "PolicyValuation",
                table: "ValuationEvents",
                column: "RolledbackByCalculationLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationLedgers_LastHandledRetroactiveChangeId",
                schema: "PolicyValuation",
                table: "ValuationLedgers",
                column: "LastHandledRetroactiveChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ValuationLedgers_PolicyId",
                schema: "PolicyValuation",
                table: "ValuationLedgers",
                column: "PolicyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "CalculationLayers",
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
