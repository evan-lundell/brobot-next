using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "brobot");

            migrationBuilder.CreateTable(
                name: "discord_user",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    birthdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    timezone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_online = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guild",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "channel",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel", x => x.id);
                    table.ForeignKey(
                        name: "FK_channel_guild_guild_id",
                        column: x => x.guild_id,
                        principalSchema: "brobot",
                        principalTable: "guild",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_user",
                schema: "brobot",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_user", x => new { x.guild_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_guild_user_discord_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild_user_guild_guild_id",
                        column: x => x.guild_id,
                        principalSchema: "brobot",
                        principalTable: "guild",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channel_user",
                schema: "brobot",
                columns: table => new
                {
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel_user", x => new { x.channel_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_channel_user_channel_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_channel_user_discord_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_message",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    send_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    created_by_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_message", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_message_channel_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scheduled_message_discord_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_channel_guild_id",
                schema: "brobot",
                table: "channel",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_channel_user_user_id",
                schema: "brobot",
                table: "channel_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_user_user_id",
                schema: "brobot",
                table: "guild_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_message_channel_id",
                schema: "brobot",
                table: "scheduled_message",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_message_created_by_id",
                schema: "brobot",
                table: "scheduled_message",
                column: "created_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "channel_user",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "guild_user",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "scheduled_message",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "channel",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "discord_user",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "guild",
                schema: "brobot");
        }
    }
}
