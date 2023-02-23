using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class HotOp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hot_op",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hot_op", x => x.id);
                    table.ForeignKey(
                        name: "FK_hot_op_channel_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hot_op_discord_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hot_op_session",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    hot_op_id = table.Column<int>(type: "integer", nullable: false),
                    start_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hot_op_session", x => x.id);
                    table.ForeignKey(
                        name: "FK_hot_op_session_discord_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hot_op_session_hot_op_hot_op_id",
                        column: x => x.hot_op_id,
                        principalSchema: "brobot",
                        principalTable: "hot_op",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hot_op_channel_id",
                schema: "brobot",
                table: "hot_op",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_hot_op_user_id",
                schema: "brobot",
                table: "hot_op",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_hot_op_session_hot_op_id",
                schema: "brobot",
                table: "hot_op_session",
                column: "hot_op_id");

            migrationBuilder.CreateIndex(
                name: "IX_hot_op_session_user_id",
                schema: "brobot",
                table: "hot_op_session",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hot_op_session",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "hot_op",
                schema: "brobot");
        }
    }
}
