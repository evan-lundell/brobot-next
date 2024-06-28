#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "length",
                schema: "brobot",
                table: "playlist_song");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "length",
                schema: "brobot",
                table: "playlist_song",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
