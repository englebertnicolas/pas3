using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.ActuarialEngine.Persistence.Migrations {
    /// <inheritdoc />
    public partial class V_1_0_0 : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.EnsureSchema(
                name: "Actu");

            migrationBuilder.CreateTable(
                name: "__RebusInbox",
                schema: "Actu",
                columns: table => new {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK___RebusInbox", x => x.MessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX___RebusInbox_ProcessedAt",
                schema: "Actu",
                table: "__RebusInbox",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "__RebusInbox",
                schema: "Actu");
        }
    }
}
