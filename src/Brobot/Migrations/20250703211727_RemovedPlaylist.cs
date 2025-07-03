using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class RemovedPlaylist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "playlist_song",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "playlist",
                schema: "brobot");

            migrationBuilder.AlterColumn<string>(
                name: "word",
                schema: "brobot",
                table: "stop_word",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "identity_user_id",
                schema: "brobot",
                table: "discord_user",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "word",
                schema: "brobot",
                table: "stop_word",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "identity_user_id",
                schema: "brobot",
                table: "discord_user",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "playlist",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playlist", x => x.id);
                    table.ForeignKey(
                        name: "FK_playlist_discord_user_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "playlist_song",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    playlist_id = table.Column<int>(type: "integer", nullable: false),
                    artist = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playlist_song", x => x.id);
                    table.ForeignKey(
                        name: "FK_playlist_song_playlist_playlist_id",
                        column: x => x.playlist_id,
                        principalSchema: "brobot",
                        principalTable: "playlist",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_playlist_discord_user_id",
                schema: "brobot",
                table: "playlist",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_playlist_song_playlist_id_order",
                schema: "brobot",
                table: "playlist_song",
                columns: new[] { "playlist_id", "order" },
                unique: true);
        }
    }
}
