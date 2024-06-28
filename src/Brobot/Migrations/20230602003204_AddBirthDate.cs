#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "birthdate",
                schema: "brobot",
                table: "discord_user",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birthdate",
                schema: "brobot",
                table: "discord_user");
        }
    }
}
