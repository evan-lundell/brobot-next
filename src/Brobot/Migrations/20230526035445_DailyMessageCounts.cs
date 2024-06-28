#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class DailyMessageCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_message_count",
                schema: "brobot",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    count_date = table.Column<DateOnly>(type: "date", nullable: false),
                    message_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_message_count", x => new { x.user_id, x.count_date });
                    table.ForeignKey(
                        name: "FK_daily_message_count_discord_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_message_count",
                schema: "brobot");
        }
    }
}
