#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class StopWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "monthly_word_cloud",
                schema: "brobot",
                table: "channel",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "stop_word",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    word = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stop_word", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stop_word_word",
                schema: "brobot",
                table: "stop_word",
                column: "word",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stop_word",
                schema: "brobot");

            migrationBuilder.DropColumn(
                name: "monthly_word_cloud",
                schema: "brobot",
                table: "channel");
        }
    }
}
