using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    /// <inheritdoc />
    public partial class ANewStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "brobot");

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
                name: "secret_santa_groups",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_santa_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stop_words",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    word = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stop_words", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "versions",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_versions", x => x.id);
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
                name: "channel_discord_users",
                schema: "brobot",
                columns: table => new
                {
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel_discord_users", x => new { x.channel_id, x.discord_user_id });
                });

            migrationBuilder.CreateTable(
                name: "channels",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    monthly_word_cloud = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    timezone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValue: "america/chicago"),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "discord_users",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    birthdate = table.Column<DateOnly>(type: "date", nullable: true),
                    timezone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_online = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    primary_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_discord_users_channels_primary_channel_id",
                        column: x => x.primary_channel_id,
                        principalSchema: "brobot",
                        principalTable: "channels",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "guilds",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    primary_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.id);
                    table.ForeignKey(
                        name: "FK_guilds_channels_primary_channel_id",
                        column: x => x.primary_channel_id,
                        principalSchema: "brobot",
                        principalTable: "channels",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stat_periods",
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
                    table.PrimaryKey("PK_stat_periods", x => x.id);
                    table.ForeignKey(
                        name: "FK_stat_periods_channels_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_message_counts",
                schema: "brobot",
                columns: table => new
                {
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    count_date = table.Column<DateOnly>(type: "date", nullable: false),
                    message_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_message_counts", x => new { x.discord_user_id, x.channel_id, x.count_date });
                    table.ForeignKey(
                        name: "FK_daily_message_counts_channels_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_daily_message_counts_discord_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hot_ops",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hot_ops", x => x.id);
                    table.ForeignKey(
                        name: "FK_hot_ops_channels_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hot_ops_discord_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_messages",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    send_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sent_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    created_by_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_messages_channels_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "brobot",
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scheduled_messages_discord_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "secret_santa_group_users",
                schema: "brobot",
                columns: table => new
                {
                    secret_santa_group_id = table.Column<int>(type: "integer", nullable: false),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_santa_group_users", x => new { x.discord_user_id, x.secret_santa_group_id });
                    table.ForeignKey(
                        name: "FK_secret_santa_group_users_discord_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_secret_santa_group_users_secret_santa_groups_secret_santa_g~",
                        column: x => x.secret_santa_group_id,
                        principalSchema: "brobot",
                        principalTable: "secret_santa_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "secret_santa_pairs",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    secret_santa_group_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    giver_discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    recipient_discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_santa_pairs", x => x.id);
                    table.ForeignKey(
                        name: "FK_secret_santa_pairs_discord_users_giver_discord_user_id",
                        column: x => x.giver_discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_secret_santa_pairs_discord_users_recipient_discord_user_id",
                        column: x => x.recipient_discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_secret_santa_pairs_secret_santa_groups_secret_santa_group_id",
                        column: x => x.secret_santa_group_id,
                        principalSchema: "brobot",
                        principalTable: "secret_santa_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "guild_discord_users",
                schema: "brobot",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_discord_users", x => new { x.guild_id, x.discord_user_id });
                    table.ForeignKey(
                        name: "FK_guild_discord_users_discord_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild_discord_users_guilds_guild_id",
                        column: x => x.guild_id,
                        principalSchema: "brobot",
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_message_counts",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    stat_period_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_message_counts", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_message_counts_discord_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_message_counts_stat_periods_stat_period_id",
                        column: x => x.stat_period_id,
                        principalSchema: "brobot",
                        principalTable: "stat_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "word_counts",
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
                    table.PrimaryKey("PK_word_counts", x => x.id);
                    table.ForeignKey(
                        name: "FK_word_counts_stat_periods_StatPeriodId",
                        column: x => x.StatPeriodId,
                        principalSchema: "brobot",
                        principalTable: "stat_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hot_op_sessions",
                schema: "brobot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    hot_op_id = table.Column<int>(type: "integer", nullable: false),
                    start_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hot_op_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_hot_op_sessions_discord_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalSchema: "brobot",
                        principalTable: "discord_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hot_op_sessions_hot_ops_hot_op_id",
                        column: x => x.hot_op_id,
                        principalSchema: "brobot",
                        principalTable: "hot_ops",
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

            migrationBuilder.CreateIndex(
                name: "IX_channel_discord_users_discord_user_id",
                schema: "brobot",
                table: "channel_discord_users",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_channels_guild_id",
                schema: "brobot",
                table: "channels",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_message_counts_channel_id",
                schema: "brobot",
                table: "daily_message_counts",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_discord_users_primary_channel_id",
                schema: "brobot",
                table: "discord_users",
                column: "primary_channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_discord_users_discord_user_id",
                schema: "brobot",
                table: "guild_discord_users",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_guilds_primary_channel_id",
                schema: "brobot",
                table: "guilds",
                column: "primary_channel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hot_op_sessions_discord_user_id",
                schema: "brobot",
                table: "hot_op_sessions",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_hot_op_sessions_hot_op_id",
                schema: "brobot",
                table: "hot_op_sessions",
                column: "hot_op_id");

            migrationBuilder.CreateIndex(
                name: "IX_hot_ops_channel_id",
                schema: "brobot",
                table: "hot_ops",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_hot_ops_discord_user_id",
                schema: "brobot",
                table: "hot_ops",
                column: "discord_user_id");

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
                name: "IX_scheduled_messages_channel_id",
                schema: "brobot",
                table: "scheduled_messages",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_messages_created_by_id",
                schema: "brobot",
                table: "scheduled_messages",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_group_users_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_group_users",
                column: "secret_santa_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_pairs_giver_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                column: "giver_discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_pairs_recipient_discord_user_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                column: "recipient_discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_secret_santa_pairs_secret_santa_group_id",
                schema: "brobot",
                table: "secret_santa_pairs",
                column: "secret_santa_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_stat_periods_channel_id",
                schema: "brobot",
                table: "stat_periods",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_stop_words_word",
                schema: "brobot",
                table: "stop_words",
                column: "word",
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

            migrationBuilder.CreateIndex(
                name: "IX_word_counts_StatPeriodId",
                schema: "brobot",
                table: "word_counts",
                column: "StatPeriodId");

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
                name: "FK_channels_guilds_guild_id",
                schema: "brobot",
                table: "channels",
                column: "guild_id",
                principalSchema: "brobot",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guilds_channels_primary_channel_id",
                schema: "brobot",
                table: "guilds");

            migrationBuilder.DropTable(
                name: "channel_discord_users",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "daily_message_counts",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "guild_discord_users",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "hot_op_sessions",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "scheduled_messages",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "secret_santa_group_users",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "secret_santa_pairs",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "stop_words",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "user_message_counts",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "versions",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "word_counts",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "hot_ops",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "secret_santa_groups",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "stat_periods",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "discord_users",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "channels",
                schema: "brobot");

            migrationBuilder.DropTable(
                name: "guilds",
                schema: "brobot");
        }
    }
}
