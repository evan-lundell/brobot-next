using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class ChannelTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                schema: "brobot",
                table: "channel",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "america/chicago");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timezone",
                schema: "brobot",
                table: "channel");
        }
    }
}
