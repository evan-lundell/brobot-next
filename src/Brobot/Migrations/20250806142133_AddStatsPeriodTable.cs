using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class AddStatsPeriodTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stat_period",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stat_period", x => x.id);
                    table.ForeignKey(
                        name: "FK_stat_period_channel_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_message_count",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    stat_period_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_message_count", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_message_count_discord_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_message_count_stat_period_stat_period_id",
                        column: x => x.stat_period_id,
                        principalSchema: "brobot",
                        principalTable: "stat_period",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "word_count",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    word = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    StatPeriodId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_word_count", x => x.id);
                    table.ForeignKey(
                        name: "FK_word_count_stat_period_StatPeriodId",
                        column: x => x.StatPeriodId,
                        principalSchema: "brobot",
                        principalTable: "stat_period",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stat_period_channel_id",
                schema: "brobot",
                table: "stat_period",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_message_count_stat_period_id",
                schema: "brobot",
                table: "user_message_count",
                column: "stat_period_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_message_count_user_id",
                schema: "brobot",
                table: "user_message_count",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_word_count_StatPeriodId",
                schema: "brobot",
                table: "word_count",
                column: "StatPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_message_count",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "word_count",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "stat_period",
                schema: "brobot");
        }
    }
}
