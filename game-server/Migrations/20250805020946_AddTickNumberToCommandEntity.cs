using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villagers.GameServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTickNumberToCommandEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TickNumber",
                table: "Commands",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TickNumber",
                table: "Commands");
        }
    }
}
