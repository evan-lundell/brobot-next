using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class SecretSanta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "secret_santa_group",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_santa_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "secret_santa_group_user",
                schema: "brobot",
                columns: table => new
                {
                    secret_santa_group_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_santa_group_user", x => new { x.user_id, x.secret_santa_group_id });
                    table.ForeignKey(
                        name: "FK_secret_santa_group_user_discord_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_secret_santa_group_user_secret_santa_group_secret_santa_gro~",
                        column: x => x.secret_santa_group_id,
                        principalSchema: "brobot",
                        principalTable: "secret_santa_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "secret_santa_pair",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    secret_santa_group_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    giver_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    recipient_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_santa_pair", x => x.id);
                    table.ForeignKey(
                        name: "FK_secret_santa_pair_discord_user_giver_user_id",
                        column: x => x.giver_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_secret_santa_pair_discord_user_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_secret_santa_pair_secret_santa_group_secret_santa_group_id",
                        column: x => x.secret_santa_group_id,
                        principalSchema: "brobot",
                        principalTable: "secret_santa_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_group_user_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_group_user",
                column: "secret_santa_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_pair_giver_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                column: "giver_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_pair_recipient_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_pair_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pair",
                column: "secret_santa_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "secret_santa_group_user",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "secret_santa_pair",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "secret_santa_group",
                schema: "brobot");
        }
    }
}
