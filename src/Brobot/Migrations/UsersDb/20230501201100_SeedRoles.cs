#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brobot.Migrations.UsersDb
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"INSERT INTO auth.\"AspNetRoles\" (\"Id\", \"Name\", \"NormalizedName\") VALUES ('{Guid.NewGuid().ToString()}', 'Admin', 'ADMIN');");
            migrationBuilder.Sql($"INSERT INTO auth.\"AspNetRoles\" (\"Id\", \"Name\", \"NormalizedName\") VALUES ('{Guid.NewGuid().ToString()}', 'User', 'USER');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM auth.\"AspNetRoles\" WHERE \"Name\" IN ('Admin', 'User');");
        }
    }
}
