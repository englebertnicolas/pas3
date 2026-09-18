using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.PolicyAdmin.Persistence.Migrations {
    /// <inheritdoc />
    public partial class V_1_0_0 : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.EnsureSchema(
                name: "PolicyAdmin");

            migrationBuilder.CreateTable(
                name: "__RebusInbox",
                schema: "PolicyAdmin",
                columns: table => new {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK___RebusInbox", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                schema: "PolicyAdmin",
                columns: table => new {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Policies", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PolicyOperations",
                schema: "PolicyAdmin",
                columns: table => new {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_PolicyOperations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PolicyOperations_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalSchema: "PolicyAdmin",
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___RebusInbox_ProcessedAt",
                schema: "PolicyAdmin",
                table: "__RebusInbox",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Status",
                schema: "PolicyAdmin",
                table: "Policies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyOperations_PolicyId",
                schema: "PolicyAdmin",
                table: "PolicyOperations",
                column: "PolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "__RebusInbox",
                schema: "PolicyAdmin");

            migrationBuilder.DropTable(
                name: "PolicyOperations",
                schema: "PolicyAdmin");

            migrationBuilder.DropTable(
                name: "Policies",
                schema: "PolicyAdmin");
        }
    }
}
