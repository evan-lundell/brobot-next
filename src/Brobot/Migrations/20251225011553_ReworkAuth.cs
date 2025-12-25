using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class ReworkAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop all foreign keys that reference tables being renamed
            migrationBuilder.DropForeignKey(
                name: "FK_channel_guild_guild_id",
                schema: "brobot",
                table: "channel");

            migrationBuilder.DropForeignKey(
                name: "FK_daily_message_count_channel_channel_id",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.DropForeignKey(
                name: "FK_daily_message_count_discord_user_user_id",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_channel_primary_channel_id",
                schema: "brobot",
                table: "guild");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_op_channel_channel_id",
                schema: "brobot",
                table: "hot_op");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_op_discord_user_user_id",
                schema: "brobot",
                table: "hot_op");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_op_session_discord_user_user_id",
                schema: "brobot",
                table: "hot_op_session");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_op_session_hot_op_hot_op_id",
                schema: "brobot",
                table: "hot_op_session");

            migrationBuilder.DropForeignKey(
                name: "FK_scheduled_message_channel_channel_id",
                schema: "brobot",
                table: "scheduled_message");

            migrationBuilder.DropForeignKey(
                name: "FK_scheduled_message_discord_user_created_by_id",
                schema: "brobot",
                table: "scheduled_message");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_pair_discord_user_giver_user_id",
                schema: "brobot",
                table: "secret_santa_pair");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_pair_discord_user_recipient_user_id",
                schema: "brobot",
                table: "secret_santa_pair");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_pair_secret_santa_group_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pair");

            migrationBuilder.DropForeignKey(
                name: "FK_stat_period_channel_channel_id",
                schema: "brobot",
                table: "stat_period");

            migrationBuilder.DropForeignKey(
                name: "FK_word_count_stat_period_StatPeriodId",
                schema: "brobot",
                table: "word_count");

            migrationBuilder.DropForeignKey(
                name: "FK_channel_user_channel_channel_id",
                schema: "brobot",
                table: "channel_user");

            migrationBuilder.DropForeignKey(
                name: "FK_channel_user_discord_user_user_id",
                schema: "brobot",
                table: "channel_user");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_user_discord_user_user_id",
                schema: "brobot",
                table: "guild_user");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_user_guild_guild_id",
                schema: "brobot",
                table: "guild_user");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_group_user_discord_user_user_id",
                schema: "brobot",
                table: "secret_santa_group_user");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_group_user_secret_santa_group_secret_santa_gro~",
                schema: "brobot",
                table: "secret_santa_group_user");

            migrationBuilder.DropForeignKey(
                name: "FK_user_message_count_discord_user_user_id",
                schema: "brobot",
                table: "user_message_count");

            migrationBuilder.DropForeignKey(
                name: "FK_user_message_count_stat_period_stat_period_id",
                schema: "brobot",
                table: "user_message_count");

            migrationBuilder.DropForeignKey(
                name: "FK_discord_user_IdentityUser_AspNetUsers_id",
                schema: "brobot",
                table: "discord_user");

            migrationBuilder.DropForeignKey(
                name: "FK_discord_user_channel_primary_channel_id",
                schema: "brobot",
                table: "discord_user");

            // Step 2: Drop primary keys on tables that need column renames in their PK
            migrationBuilder.DropPrimaryKey(
                name: "PK_word_count",
                schema: "brobot",
                table: "word_count");

            migrationBuilder.DropPrimaryKey(
                name: "PK_version",
                schema: "brobot",
                table: "version");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stop_word",
                schema: "brobot",
                table: "stop_word");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stat_period",
                schema: "brobot",
                table: "stat_period");

            migrationBuilder.DropPrimaryKey(
                name: "PK_secret_santa_pair",
                schema: "brobot",
                table: "secret_santa_pair");

            migrationBuilder.DropPrimaryKey(
                name: "PK_secret_santa_group",
                schema: "brobot",
                table: "secret_santa_group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_scheduled_message",
                schema: "brobot",
                table: "scheduled_message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hot_op_session",
                schema: "brobot",
                table: "hot_op_session");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hot_op",
                schema: "brobot",
                table: "hot_op");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild",
                schema: "brobot",
                table: "guild");

            migrationBuilder.DropPrimaryKey(
                name: "PK_daily_message_count",
                schema: "brobot",
                table: "daily_message_count");

            migrationBuilder.DropPrimaryKey(
                name: "PK_channel",
                schema: "brobot",
                table: "channel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_discord_user",
                schema: "brobot",
                table: "discord_user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_channel_user",
                schema: "brobot",
                table: "channel_user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_user",
                schema: "brobot",
                table: "guild_user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_secret_santa_group_user",
                schema: "brobot",
                table: "secret_santa_group_user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_message_count",
                schema: "brobot",
                table: "user_message_count");

            // Step 3: Drop indexes that will be recreated
            migrationBuilder.DropIndex(
                name: "IX_channel_user_user_id",
                schema: "brobot",
                table: "channel_user");

            migrationBuilder.DropIndex(
                name: "IX_guild_user_user_id",
                schema: "brobot",
                table: "guild_user");

            migrationBuilder.DropIndex(
                name: "IX_secret_santa_group_user_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_group_user");

            migrationBuilder.DropIndex(
                name: "IX_user_message_count_stat_period_id",
                schema: "brobot",
                table: "user_message_count");

            migrationBuilder.DropIndex(
                name: "IX_user_message_count_user_id",
                schema: "brobot",
                table: "user_message_count");

            migrationBuilder.DropIndex(
                name: "IX_discord_user_identity_user_id",
                schema: "brobot",
                table: "discord_user");

            migrationBuilder.DropIndex(
                name: "IX_discord_user_primary_channel_id",
                schema: "brobot",
                table: "discord_user");

            // Step 4: Drop identity_user_id column from discord_user
            migrationBuilder.DropColumn(
                name: "identity_user_id",
                schema: "brobot",
                table: "discord_user");

            // Step 5: Rename column in join tables
            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "brobot",
                table: "channel_user",
                newName: "discord_user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "brobot",
                table: "guild_user",
                newName: "discord_user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "brobot",
                table: "secret_santa_group_user",
                newName: "discord_user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "brobot",
                table: "user_message_count",
                newName: "discord_user_id");

            // Step 9: Rename columns in other tables
            migrationBuilder.RenameColumn(
                name: "recipient_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                newName: "recipient_discord_user_id");

            migrationBuilder.RenameColumn(
                name: "giver_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                newName: "giver_discord_user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "brobot",
                table: "hot_op_session",
                newName: "discord_user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "brobot",
                table: "hot_op",
                newName: "discord_user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "brobot",
                table: "daily_message_count",
                newName: "discord_user_id");

            // Step 10: Rename all tables to plural
            migrationBuilder.RenameTable(
                name: "word_count",
                schema: "brobot",
                newName: "word_counts",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "version",
                schema: "brobot",
                newName: "versions",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "stop_word",
                schema: "brobot",
                newName: "stop_words",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "stat_period",
                schema: "brobot",
                newName: "stat_periods",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "secret_santa_pair",
                schema: "brobot",
                newName: "secret_santa_pairs",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "secret_santa_group",
                schema: "brobot",
                newName: "secret_santa_groups",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "scheduled_message",
                schema: "brobot",
                newName: "scheduled_messages",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "hot_op_session",
                schema: "brobot",
                newName: "hot_op_sessions",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "hot_op",
                schema: "brobot",
                newName: "hot_ops",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "guild",
                schema: "brobot",
                newName: "guilds",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "daily_message_count",
                schema: "brobot",
                newName: "daily_message_counts",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "channel",
                schema: "brobot",
                newName: "channels",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "discord_user",
                schema: "brobot",
                newName: "discord_users",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "channel_user",
                schema: "brobot",
                newName: "channel_discord_users",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "guild_user",
                schema: "brobot",
                newName: "guild_discord_users",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "secret_santa_group_user",
                schema: "brobot",
                newName: "secret_santa_group_users",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "user_message_count",
                schema: "brobot",
                newName: "user_message_counts",
                newSchema: "brobot");

            // Step 8: Drop old auth schema Identity tables
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "auth");

            // Step 9: Drop the EF migrations history table in auth schema
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS auth.""__EFMigrationsHistory"";");

            // Step 10: Drop the auth schema
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS auth;");

            // Step 10: Rename indexes
            migrationBuilder.RenameIndex(
                name: "IX_word_count_StatPeriodId",
                schema: "brobot",
                table: "word_counts",
                newName: "IX_word_counts_StatPeriodId");

            migrationBuilder.RenameIndex(
                name: "IX_stop_word_word",
                schema: "brobot",
                table: "stop_words",
                newName: "IX_stop_words_word");

            migrationBuilder.RenameIndex(
                name: "IX_stat_period_channel_id",
                schema: "brobot",
                table: "stat_periods",
                newName: "IX_stat_periods_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_secret_santa_pair_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                newName: "IX_secret_santa_pairs_secret_santa_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_secret_santa_pair_recipient_user_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                newName: "IX_secret_santa_pairs_recipient_discord_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_secret_santa_pair_giver_user_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                newName: "IX_secret_santa_pairs_giver_discord_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_scheduled_message_created_by_id",
                schema: "brobot",
                table: "scheduled_messages",
                newName: "IX_scheduled_messages_created_by_id");

            migrationBuilder.RenameIndex(
                name: "IX_scheduled_message_channel_id",
                schema: "brobot",
                table: "scheduled_messages",
                newName: "IX_scheduled_messages_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_op_session_user_id",
                schema: "brobot",
                table: "hot_op_sessions",
                newName: "IX_hot_op_sessions_discord_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_op_session_hot_op_id",
                schema: "brobot",
                table: "hot_op_sessions",
                newName: "IX_hot_op_sessions_hot_op_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_op_user_id",
                schema: "brobot",
                table: "hot_ops",
                newName: "IX_hot_ops_discord_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_op_channel_id",
                schema: "brobot",
                table: "hot_ops",
                newName: "IX_hot_ops_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_guild_primary_channel_id",
                schema: "brobot",
                table: "guilds",
                newName: "IX_guilds_primary_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_daily_message_count_channel_id",
                schema: "brobot",
                table: "daily_message_counts",
                newName: "IX_daily_message_counts_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_channel_guild_id",
                schema: "brobot",
                table: "channels",
                newName: "IX_channels_guild_id");

            // Step 12: Add primary keys back
            migrationBuilder.AddPrimaryKey(
                name: "PK_word_counts",
                schema: "brobot",
                table: "word_counts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_versions",
                schema: "brobot",
                table: "versions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stop_words",
                schema: "brobot",
                table: "stop_words",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stat_periods",
                schema: "brobot",
                table: "stat_periods",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_secret_santa_pairs",
                schema: "brobot",
                table: "secret_santa_pairs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_secret_santa_groups",
                schema: "brobot",
                table: "secret_santa_groups",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_scheduled_messages",
                schema: "brobot",
                table: "scheduled_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hot_op_sessions",
                schema: "brobot",
                table: "hot_op_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hot_ops",
                schema: "brobot",
                table: "hot_ops",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guilds",
                schema: "brobot",
                table: "guilds",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_daily_message_counts",
                schema: "brobot",
                table: "daily_message_counts",
                columns: new[] { "discord_user_id", "channel_id", "count_date" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_channels",
                schema: "brobot",
                table: "channels",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_discord_users",
                schema: "brobot",
                table: "discord_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_channel_discord_users",
                schema: "brobot",
                table: "channel_discord_users",
                columns: new[] { "channel_id", "discord_user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_discord_users",
                schema: "brobot",
                table: "guild_discord_users",
                columns: new[] { "guild_id", "discord_user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_secret_santa_group_users",
                schema: "brobot",
                table: "secret_santa_group_users",
                columns: new[] { "discord_user_id", "secret_santa_group_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_message_counts",
                schema: "brobot",
                table: "user_message_counts",
                column: "id");

            // Step 12: Create new Identity tables
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_discord_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_claims_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_user_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_user_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Step 14: Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_discord_users_primary_channel_id",
                schema: "brobot",
                table: "discord_users",
                column: "primary_channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_channel_discord_users_discord_user_id",
                schema: "brobot",
                table: "channel_discord_users",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_discord_users_discord_user_id",
                schema: "brobot",
                table: "guild_discord_users",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_group_users_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_group_users",
                column: "secret_santa_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_message_counts_discord_user_id",
                schema: "brobot",
                table: "user_message_counts",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_message_counts_stat_period_id",
                schema: "brobot",
                table: "user_message_counts",
                column: "stat_period_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_claims_role_id",
                table: "role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_claims_user_id",
                table: "user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_logins_user_id",
                table: "user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "IX_users_discord_user_id",
                table: "users",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            // Step 15: Add foreign keys back
            migrationBuilder.AddForeignKey(
                name: "FK_channels_guilds_guild_id",
                schema: "brobot",
                table: "channels",
                column: "guild_id",
                principalSchema: "brobot",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_daily_message_counts_channels_channel_id",
                schema: "brobot",
                table: "daily_message_counts",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_daily_message_counts_discord_users_discord_user_id",
                schema: "brobot",
                table: "daily_message_counts",
                column: "discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guilds_channels_primary_channel_id",
                schema: "brobot",
                table: "guilds",
                column: "primary_channel_id",
                principalSchema: "brobot",
                principalTable: "channels",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_hot_op_sessions_discord_users_discord_user_id",
                schema: "brobot",
                table: "hot_op_sessions",
                column: "discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hot_op_sessions_hot_ops_hot_op_id",
                schema: "brobot",
                table: "hot_op_sessions",
                column: "hot_op_id",
                principalSchema: "brobot",
                principalTable: "hot_ops",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hot_ops_channels_channel_id",
                schema: "brobot",
                table: "hot_ops",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hot_ops_discord_users_discord_user_id",
                schema: "brobot",
                table: "hot_ops",
                column: "discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_scheduled_messages_channels_channel_id",
                schema: "brobot",
                table: "scheduled_messages",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_scheduled_messages_discord_users_created_by_id",
                schema: "brobot",
                table: "scheduled_messages",
                column: "created_by_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_pairs_discord_users_giver_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                column: "giver_discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_pairs_discord_users_recipient_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                column: "recipient_discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_pairs_secret_santa_groups_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                column: "secret_santa_group_id",
                principalSchema: "brobot",
                principalTable: "secret_santa_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_stat_periods_channels_channel_id",
                schema: "brobot",
                table: "stat_periods",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_word_counts_stat_periods_StatPeriodId",
                schema: "brobot",
                table: "word_counts",
                column: "StatPeriodId",
                principalSchema: "brobot",
                principalTable: "stat_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_discord_users_channels_primary_channel_id",
                schema: "brobot",
                table: "discord_users",
                column: "primary_channel_id",
                principalSchema: "brobot",
                principalTable: "channels",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_channel_discord_users_channels_channel_id",
                schema: "brobot",
                table: "channel_discord_users",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_channel_discord_users_discord_users_discord_user_id",
                schema: "brobot",
                table: "channel_discord_users",
                column: "discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_discord_users_discord_users_discord_user_id",
                schema: "brobot",
                table: "guild_discord_users",
                column: "discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_discord_users_guilds_guild_id",
                schema: "brobot",
                table: "guild_discord_users",
                column: "guild_id",
                principalSchema: "brobot",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_group_users_discord_users_discord_user_id",
                schema: "brobot",
                table: "secret_santa_group_users",
                column: "discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_group_users_secret_santa_groups_secret_santa_g~",
                schema: "brobot",
                table: "secret_santa_group_users",
                column: "secret_santa_group_id",
                principalSchema: "brobot",
                principalTable: "secret_santa_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_message_counts_discord_users_discord_user_id",
                schema: "brobot",
                table: "user_message_counts",
                column: "discord_user_id",
                principalSchema: "brobot",
                principalTable: "discord_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_message_counts_stat_periods_stat_period_id",
                schema: "brobot",
                table: "user_message_counts",
                column: "stat_period_id",
                principalSchema: "brobot",
                principalTable: "stat_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop new Identity tables (in order due to FK dependencies)
            migrationBuilder.DropTable(name: "role_claims");
            migrationBuilder.DropTable(name: "user_claims");
            migrationBuilder.DropTable(name: "user_logins");
            migrationBuilder.DropTable(name: "user_roles");
            migrationBuilder.DropTable(name: "user_tokens");
            migrationBuilder.DropTable(name: "roles");
            migrationBuilder.DropTable(name: "users");

            // Step 2: Recreate auth schema and auth.AspNet* tables
            migrationBuilder.EnsureSchema(name: "auth");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "auth",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "auth",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "auth",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "auth",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "auth",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "auth",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "auth",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "auth",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            // Seed roles
            migrationBuilder.Sql($"INSERT INTO auth.\"AspNetRoles\" (\"Id\", \"Name\", \"NormalizedName\") VALUES ('{Guid.NewGuid().ToString()}', 'Admin', 'ADMIN');");
            migrationBuilder.Sql($"INSERT INTO auth.\"AspNetRoles\" (\"Id\", \"Name\", \"NormalizedName\") VALUES ('{Guid.NewGuid().ToString()}', 'User', 'USER');");

            // Recreate __EFMigrationsHistory table in auth schema
            migrationBuilder.Sql(@"
                CREATE TABLE auth.""__EFMigrationsHistory"" (
                    ""MigrationId"" character varying(150) NOT NULL,
                    ""ProductVersion"" character varying(32) NOT NULL,
                    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                );
                INSERT INTO auth.""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES ('20230501201043_InitialUserMigration', '7.0.5');
                INSERT INTO auth.""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES ('20230501201100_SeedRoles', '7.0.5');
            ");

            // Step 2: Drop all foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_channels_guilds_guild_id",
                schema: "brobot",
                table: "channels");

            migrationBuilder.DropForeignKey(
                name: "FK_daily_message_counts_channels_channel_id",
                schema: "brobot",
                table: "daily_message_counts");

            migrationBuilder.DropForeignKey(
                name: "FK_daily_message_counts_discord_users_discord_user_id",
                schema: "brobot",
                table: "daily_message_counts");

            migrationBuilder.DropForeignKey(
                name: "FK_guilds_channels_primary_channel_id",
                schema: "brobot",
                table: "guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_op_sessions_discord_users_discord_user_id",
                schema: "brobot",
                table: "hot_op_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_op_sessions_hot_ops_hot_op_id",
                schema: "brobot",
                table: "hot_op_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_ops_channels_channel_id",
                schema: "brobot",
                table: "hot_ops");

            migrationBuilder.DropForeignKey(
                name: "FK_hot_ops_discord_users_discord_user_id",
                schema: "brobot",
                table: "hot_ops");

            migrationBuilder.DropForeignKey(
                name: "FK_scheduled_messages_channels_channel_id",
                schema: "brobot",
                table: "scheduled_messages");

            migrationBuilder.DropForeignKey(
                name: "FK_scheduled_messages_discord_users_created_by_id",
                schema: "brobot",
                table: "scheduled_messages");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_pairs_discord_users_giver_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pairs");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_pairs_discord_users_recipient_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pairs");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_pairs_secret_santa_groups_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pairs");

            migrationBuilder.DropForeignKey(
                name: "FK_stat_periods_channels_channel_id",
                schema: "brobot",
                table: "stat_periods");

            migrationBuilder.DropForeignKey(
                name: "FK_word_counts_stat_periods_StatPeriodId",
                schema: "brobot",
                table: "word_counts");

            migrationBuilder.DropForeignKey(
                name: "FK_discord_users_channels_primary_channel_id",
                schema: "brobot",
                table: "discord_users");

            migrationBuilder.DropForeignKey(
                name: "FK_channel_discord_users_channels_channel_id",
                schema: "brobot",
                table: "channel_discord_users");

            migrationBuilder.DropForeignKey(
                name: "FK_channel_discord_users_discord_users_discord_user_id",
                schema: "brobot",
                table: "channel_discord_users");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_discord_users_discord_users_discord_user_id",
                schema: "brobot",
                table: "guild_discord_users");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_discord_users_guilds_guild_id",
                schema: "brobot",
                table: "guild_discord_users");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_group_users_discord_users_discord_user_id",
                schema: "brobot",
                table: "secret_santa_group_users");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_santa_group_users_secret_santa_groups_secret_santa_g~",
                schema: "brobot",
                table: "secret_santa_group_users");

            migrationBuilder.DropForeignKey(
                name: "FK_user_message_counts_discord_users_discord_user_id",
                schema: "brobot",
                table: "user_message_counts");

            migrationBuilder.DropForeignKey(
                name: "FK_user_message_counts_stat_periods_stat_period_id",
                schema: "brobot",
                table: "user_message_counts");

            // Step 3: Drop primary keys
            migrationBuilder.DropPrimaryKey(
                name: "PK_word_counts",
                schema: "brobot",
                table: "word_counts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_versions",
                schema: "brobot",
                table: "versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stop_words",
                schema: "brobot",
                table: "stop_words");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stat_periods",
                schema: "brobot",
                table: "stat_periods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_secret_santa_pairs",
                schema: "brobot",
                table: "secret_santa_pairs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_secret_santa_groups",
                schema: "brobot",
                table: "secret_santa_groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_scheduled_messages",
                schema: "brobot",
                table: "scheduled_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hot_ops",
                schema: "brobot",
                table: "hot_ops");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hot_op_sessions",
                schema: "brobot",
                table: "hot_op_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guilds",
                schema: "brobot",
                table: "guilds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_daily_message_counts",
                schema: "brobot",
                table: "daily_message_counts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_channels",
                schema: "brobot",
                table: "channels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_discord_users",
                schema: "brobot",
                table: "discord_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_channel_discord_users",
                schema: "brobot",
                table: "channel_discord_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_discord_users",
                schema: "brobot",
                table: "guild_discord_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_secret_santa_group_users",
                schema: "brobot",
                table: "secret_santa_group_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_message_counts",
                schema: "brobot",
                table: "user_message_counts");

            // Step 4: Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_discord_users_primary_channel_id",
                schema: "brobot",
                table: "discord_users");

            migrationBuilder.DropIndex(
                name: "IX_channel_discord_users_discord_user_id",
                schema: "brobot",
                table: "channel_discord_users");

            migrationBuilder.DropIndex(
                name: "IX_guild_discord_users_discord_user_id",
                schema: "brobot",
                table: "guild_discord_users");

            migrationBuilder.DropIndex(
                name: "IX_secret_santa_group_users_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_group_users");

            migrationBuilder.DropIndex(
                name: "IX_user_message_counts_discord_user_id",
                schema: "brobot",
                table: "user_message_counts");

            migrationBuilder.DropIndex(
                name: "IX_user_message_counts_stat_period_id",
                schema: "brobot",
                table: "user_message_counts");

            // Step 5: Rename tables back to singular
            migrationBuilder.RenameTable(
                name: "word_counts",
                schema: "brobot",
                newName: "word_count",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "versions",
                schema: "brobot",
                newName: "version",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "stop_words",
                schema: "brobot",
                newName: "stop_word",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "stat_periods",
                schema: "brobot",
                newName: "stat_period",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "secret_santa_pairs",
                schema: "brobot",
                newName: "secret_santa_pair",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "secret_santa_groups",
                schema: "brobot",
                newName: "secret_santa_group",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "scheduled_messages",
                schema: "brobot",
                newName: "scheduled_message",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "hot_op_sessions",
                schema: "brobot",
                newName: "hot_op_session",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "hot_ops",
                schema: "brobot",
                newName: "hot_op",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "guilds",
                schema: "brobot",
                newName: "guild",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "daily_message_counts",
                schema: "brobot",
                newName: "daily_message_count",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "channels",
                schema: "brobot",
                newName: "channel",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "discord_users",
                schema: "brobot",
                newName: "discord_user",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "channel_discord_users",
                schema: "brobot",
                newName: "channel_user",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "guild_discord_users",
                schema: "brobot",
                newName: "guild_user",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "secret_santa_group_users",
                schema: "brobot",
                newName: "secret_santa_group_user",
                newSchema: "brobot");

            migrationBuilder.RenameTable(
                name: "user_message_counts",
                schema: "brobot",
                newName: "user_message_count",
                newSchema: "brobot");

            // Step 6: Rename columns back
            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                schema: "brobot",
                table: "channel_user",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                schema: "brobot",
                table: "guild_user",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                schema: "brobot",
                table: "secret_santa_group_user",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                schema: "brobot",
                table: "user_message_count",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "recipient_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                newName: "recipient_user_id");

            migrationBuilder.RenameColumn(
                name: "giver_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                newName: "giver_user_id");

            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                schema: "brobot",
                table: "hot_op_session",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                schema: "brobot",
                table: "hot_op",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                schema: "brobot",
                table: "daily_message_count",
                newName: "user_id");

            // Step 7: Rename indexes back
            migrationBuilder.RenameIndex(
                name: "IX_word_counts_StatPeriodId",
                schema: "brobot",
                table: "word_count",
                newName: "IX_word_count_StatPeriodId");

            migrationBuilder.RenameIndex(
                name: "IX_stop_words_word",
                schema: "brobot",
                table: "stop_word",
                newName: "IX_stop_word_word");

            migrationBuilder.RenameIndex(
                name: "IX_stat_periods_channel_id",
                schema: "brobot",
                table: "stat_period",
                newName: "IX_stat_period_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_secret_santa_pairs_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pair",
                newName: "IX_secret_santa_pair_secret_santa_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_secret_santa_pairs_recipient_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                newName: "IX_secret_santa_pair_recipient_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_secret_santa_pairs_giver_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                newName: "IX_secret_santa_pair_giver_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_scheduled_messages_created_by_id",
                schema: "brobot",
                table: "scheduled_message",
                newName: "IX_scheduled_message_created_by_id");

            migrationBuilder.RenameIndex(
                name: "IX_scheduled_messages_channel_id",
                schema: "brobot",
                table: "scheduled_message",
                newName: "IX_scheduled_message_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_op_sessions_discord_user_id",
                schema: "brobot",
                table: "hot_op_session",
                newName: "IX_hot_op_session_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_op_sessions_hot_op_id",
                schema: "brobot",
                table: "hot_op_session",
                newName: "IX_hot_op_session_hot_op_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_ops_discord_user_id",
                schema: "brobot",
                table: "hot_op",
                newName: "IX_hot_op_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_hot_ops_channel_id",
                schema: "brobot",
                table: "hot_op",
                newName: "IX_hot_op_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_guilds_primary_channel_id",
                schema: "brobot",
                table: "guild",
                newName: "IX_guild_primary_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_daily_message_counts_channel_id",
                schema: "brobot",
                table: "daily_message_count",
                newName: "IX_daily_message_count_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_channels_guild_id",
                schema: "brobot",
                table: "channel",
                newName: "IX_channel_guild_id");

            // Step 8: Add identity_user_id column back to discord_user
            migrationBuilder.AddColumn<string>(
                name: "identity_user_id",
                schema: "brobot",
                table: "discord_user",
                type: "text",
                nullable: true);

            // Step 9: Add primary keys back
            migrationBuilder.AddPrimaryKey(
                name: "PK_word_count",
                schema: "brobot",
                table: "word_count",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_version",
                schema: "brobot",
                table: "version",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stop_word",
                schema: "brobot",
                table: "stop_word",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stat_period",
                schema: "brobot",
                table: "stat_period",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_secret_santa_pair",
                schema: "brobot",
                table: "secret_santa_pair",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_secret_santa_group",
                schema: "brobot",
                table: "secret_santa_group",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_scheduled_message",
                schema: "brobot",
                table: "scheduled_message",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hot_op",
                schema: "brobot",
                table: "hot_op",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hot_op_session",
                schema: "brobot",
                table: "hot_op_session",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild",
                schema: "brobot",
                table: "guild",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_daily_message_count",
                schema: "brobot",
                table: "daily_message_count",
                columns: new[] { "user_id", "channel_id", "count_date" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_channel",
                schema: "brobot",
                table: "channel",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_discord_user",
                schema: "brobot",
                table: "discord_user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_channel_user",
                schema: "brobot",
                table: "channel_user",
                columns: new[] { "channel_id", "user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_user",
                schema: "brobot",
                table: "guild_user",
                columns: new[] { "guild_id", "user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_secret_santa_group_user",
                schema: "brobot",
                table: "secret_santa_group_user",
                columns: new[] { "user_id", "secret_santa_group_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_message_count",
                schema: "brobot",
                table: "user_message_count",
                column: "id");

            // Step 10: Create indexes
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
                name: "IX_secret_santa_group_user_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_group_user",
                column: "secret_santa_group_id");

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
                name: "IX_discord_user_identity_user_id",
                schema: "brobot",
                table: "discord_user",
                column: "identity_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discord_user_primary_channel_id",
                schema: "brobot",
                table: "discord_user",
                column: "primary_channel_id");

            // Step 14: Add foreign keys back
            migrationBuilder.AddForeignKey(
                name: "FK_channel_guild_guild_id",
                schema: "brobot",
                table: "channel",
                column: "guild_id",
                principalSchema: "brobot",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_daily_message_count_channel_channel_id",
                schema: "brobot",
                table: "daily_message_count",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_daily_message_count_discord_user_user_id",
                schema: "brobot",
                table: "daily_message_count",
                column: "user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_channel_primary_channel_id",
                schema: "brobot",
                table: "guild",
                column: "primary_channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_hot_op_channel_channel_id",
                schema: "brobot",
                table: "hot_op",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hot_op_discord_user_user_id",
                schema: "brobot",
                table: "hot_op",
                column: "user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hot_op_session_discord_user_user_id",
                schema: "brobot",
                table: "hot_op_session",
                column: "user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hot_op_session_hot_op_hot_op_id",
                schema: "brobot",
                table: "hot_op_session",
                column: "hot_op_id",
                principalSchema: "brobot",
                principalTable: "hot_op",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_scheduled_message_channel_channel_id",
                schema: "brobot",
                table: "scheduled_message",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_scheduled_message_discord_user_created_by_id",
                schema: "brobot",
                table: "scheduled_message",
                column: "created_by_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_pair_discord_user_giver_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                column: "giver_user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_pair_discord_user_recipient_user_id",
                schema: "brobot",
                table: "secret_santa_pair",
                column: "recipient_user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_pair_secret_santa_group_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pair",
                column: "secret_santa_group_id",
                principalSchema: "brobot",
                principalTable: "secret_santa_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_stat_period_channel_channel_id",
                schema: "brobot",
                table: "stat_period",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_word_count_stat_period_StatPeriodId",
                schema: "brobot",
                table: "word_count",
                column: "StatPeriodId",
                principalSchema: "brobot",
                principalTable: "stat_period",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_discord_user_IdentityUser_AspNetUsers_id",
                schema: "brobot",
                table: "discord_user",
                column: "identity_user_id",
                principalSchema: "auth",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_discord_user_channel_primary_channel_id",
                schema: "brobot",
                table: "discord_user",
                column: "primary_channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_channel_user_channel_channel_id",
                schema: "brobot",
                table: "channel_user",
                column: "channel_id",
                principalSchema: "brobot",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_channel_user_discord_user_user_id",
                schema: "brobot",
                table: "channel_user",
                column: "user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_user_discord_user_user_id",
                schema: "brobot",
                table: "guild_user",
                column: "user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_user_guild_guild_id",
                schema: "brobot",
                table: "guild_user",
                column: "guild_id",
                principalSchema: "brobot",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_group_user_discord_user_user_id",
                schema: "brobot",
                table: "secret_santa_group_user",
                column: "user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_santa_group_user_secret_santa_group_secret_santa_gro~",
                schema: "brobot",
                table: "secret_santa_group_user",
                column: "secret_santa_group_id",
                principalSchema: "brobot",
                principalTable: "secret_santa_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_message_count_discord_user_user_id",
                schema: "brobot",
                table: "user_message_count",
                column: "user_id",
                principalSchema: "brobot",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_message_count_stat_period_stat_period_id",
                schema: "brobot",
                table: "user_message_count",
                column: "stat_period_id",
                principalSchema: "brobot",
                principalTable: "stat_period",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
