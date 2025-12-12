using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CĐTNDA_NhanDangBienSoXe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditLogDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                schema: "dbo",
                table: "AuditLogs",
                column: "UserId",
                principalSchema: "pr",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                schema: "dbo",
                table: "AuditLogs",
                column: "UserId",
                principalSchema: "pr",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
