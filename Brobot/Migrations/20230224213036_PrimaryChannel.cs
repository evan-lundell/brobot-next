using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class PrimaryChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "primary_channel_id",
                schema: "brobot",
                table: "discord_user",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_discord_user_primary_channel_id",
                schema: "brobot",
                table: "discord_user",
                column: "primary_channel_id");

            migrationBuilder.AddForeignKey(
                name: "FK_discord_user_channel_primary_channel_id",
                schema: "brobot",
                table: "discord_user",
                column: "primary_channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_discord_user_channel_primary_channel_id",
                schema: "brobot",
                table: "discord_user");

            migrationBuilder.DropIndex(
                name: "IX_discord_user_primary_channel_id",
                schema: "brobot",
                table: "discord_user");

            migrationBuilder.DropColumn(
                name: "primary_channel_id",
                schema: "brobot",
                table: "discord_user");
        }
    }
}
