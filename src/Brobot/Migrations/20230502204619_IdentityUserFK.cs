using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class IdentityUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_user_id",
                schema: "brobot",
                table: "discord_user",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_discord_user_identity_user_id",
                schema: "brobot",
                table: "discord_user",
                column: "identity_user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_discord_user_IdentityUser_AspNetUsers_id",
                schema: "brobot",
                table: "discord_user",
                column: "identity_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                principalSchema: "auth");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_discord_user_IdentityUser_AspNetUsers_id",
                schema: "brobot",
                table: "discord_user");

            migrationBuilder.DropIndex(
                name: "IX_discord_user_identity_user_id",
                schema: "brobot",
                table: "discord_user");

            migrationBuilder.DropColumn(
                name: "identity_user_id",
                schema: "brobot",
                table: "discord_user");
        }
    }
}
