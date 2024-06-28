#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class DailyMessageRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM daily_message_count");
            migrationBuilder.DropPrimaryKey(
                name: "PK_daily_message_count",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.AddColumn<decimal>(
                name: "channel_id",
                schema: "brobot",
                table: "daily_message_count",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_daily_message_count",
                schema: "brobot",
                table: "daily_message_count",
                columns: new[] { "user_id", "channel_id", "count_date" });

            migrationBuilder.CreateIndex(
                name: "IX_daily_message_count_channel_id",
                schema: "brobot",
                table: "daily_message_count",
                column: "channel_id");

            migrationBuilder.AddForeignKey(
                name: "FK_daily_message_count_channel_channel_id",
                schema: "brobot",
                table: "daily_message_count",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_daily_message_count_channel_channel_id",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.DropPrimaryKey(
                name: "PK_daily_message_count",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.DropIndex(
                name: "IX_daily_message_count_channel_id",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.DropColumn(
                name: "channel_id",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.AddPrimaryKey(
                name: "PK_daily_message_count",
                schema: "brobot",
                table: "daily_message_count",
                columns: new[] { "user_id", "count_date" });
        }
    }
}
