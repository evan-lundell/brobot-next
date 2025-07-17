using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "primary_channel_id",
                schema: "brobot",
                table: "guild",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_primary_channel_id",
                schema: "brobot",
                table: "guild",
                column: "primary_channel_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_channel_primary_channel_id",
                schema: "brobot",
                table: "guild",
                column: "primary_channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_channel_primary_channel_id",
                schema: "brobot",
                table: "guild");

            migrationBuilder.DropIndex(
                name: "IX_guild_primary_channel_id",
                schema: "brobot",
                table: "guild");

            migrationBuilder.DropColumn(
                name: "primary_channel_id",
                schema: "brobot",
                table: "guild");
        }
    }
}
