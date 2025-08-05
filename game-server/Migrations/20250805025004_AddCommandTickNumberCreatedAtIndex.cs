using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villagers.GameServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCommandTickNumberCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Commands_TickNumber_CreatedAt",
                table: "Commands",
                columns: new[] { "TickNumber", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Commands_TickNumber_CreatedAt",
                table: "Commands");
        }
    }
}
