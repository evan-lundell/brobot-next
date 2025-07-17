using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class RenameTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timezone",
                schema: "brobot",
                table: "channel",
                newName: "timezone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "timezone",
                schema: "brobot",
                table: "channel",
                newName: "Timezone");
        }
    }
}
