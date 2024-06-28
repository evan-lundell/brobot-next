﻿// <auto-generated />

#nullable disable

using Brobot.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brobot.Migrations
{
    [DbContext(typeof(BrobotDbContext))]
    [Migration("20230726033113_DailyMessageRework")]
    partial class DailyMessageRework
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Brobot.Models.ChannelModel", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("id");

                    b.Property<bool>("Archived")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("archived");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("channel", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.ChannelUserModel", b =>
                {
                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("ChannelId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("channel_user", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.DailyMessageCountModel", b =>
                {
                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<DateOnly>("CountDate")
                        .HasColumnType("date")
                        .HasColumnName("count_date");

                    b.Property<int>("MessageCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("message_count");

                    b.HasKey("UserId", "ChannelId", "CountDate");

                    b.HasIndex("ChannelId");

                    b.ToTable("daily_message_count", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.GuildModel", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("id");

                    b.Property<bool>("Archived")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("archived");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("guild", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.GuildUserModel", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("GuildId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("guild_user", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.HotOpModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<DateTimeOffset>("EndDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_date");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_date");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("ChannelId");

                    b.HasIndex("UserId");

                    b.ToTable("hot_op", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.HotOpSessionModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("EndDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_date_time");

                    b.Property<int>("HotOpId")
                        .HasColumnType("integer")
                        .HasColumnName("hot_op_id");

                    b.Property<DateTimeOffset>("StartDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_date_time");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("HotOpId");

                    b.HasIndex("UserId");

                    b.ToTable("hot_op_session", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.PlaylistModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("discord_user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("playlist", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.PlaylistSongModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("artist");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name");

                    b.Property<int>("Order")
                        .HasColumnType("integer")
                        .HasColumnName("order");

                    b.Property<int>("PlaylistId")
                        .HasColumnType("integer")
                        .HasColumnName("playlist_id");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasColumnName("url");

                    b.HasKey("Id");

                    b.HasIndex("PlaylistId", "Order")
                        .IsUnique();

                    b.ToTable("playlist_song", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.ScheduledMessageModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("CreatedById")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("created_by_id");

                    b.Property<string>("MessageText")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("message_text");

                    b.Property<DateTimeOffset?>("SendDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("send_date");

                    b.Property<DateTimeOffset?>("SentDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("sent_date");

                    b.HasKey("Id");

                    b.HasIndex("ChannelId");

                    b.HasIndex("CreatedById");

                    b.ToTable("scheduled_message", "brobot");
                });

            modelBuilder.Entity("Brobot.Models.UserModel", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("id");

                    b.Property<bool>("Archived")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("archived");

                    b.Property<DateOnly?>("Birthdate")
                        .HasColumnType("date")
                        .HasColumnName("birthdate");

                    b.Property<string>("IdentityUserId")
                        .HasColumnType("text")
                        .HasColumnName("identity_user_id");

                    b.Property<DateTimeOffset?>("LastOnline")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_online");

                    b.Property<decimal?>("PrimaryChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("primary_channel_id");

                    b.Property<string>("Timezone")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("timezone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.HasIndex("IdentityUserId")
                        .IsUnique();

                    b.HasIndex("PrimaryChannelId");

                    b.ToTable("discord_user", "brobot");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("text");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("IdentityUser");
                });

            modelBuilder.Entity("Brobot.Models.ChannelModel", b =>
                {
                    b.HasOne("Brobot.Models.GuildModel", "Guild")
                        .WithMany("Channels")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Brobot.Models.ChannelUserModel", b =>
                {
                    b.HasOne("Brobot.Models.ChannelModel", "Channel")
                        .WithMany("ChannelUsers")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brobot.Models.UserModel", "User")
                        .WithMany("ChannelUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brobot.Models.DailyMessageCountModel", b =>
                {
                    b.HasOne("Brobot.Models.ChannelModel", "Channel")
                        .WithMany("DailyMessageCounts")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brobot.Models.UserModel", "User")
                        .WithMany("DailyCounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brobot.Models.GuildUserModel", b =>
                {
                    b.HasOne("Brobot.Models.GuildModel", "Guild")
                        .WithMany("GuildUsers")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brobot.Models.UserModel", "User")
                        .WithMany("GuildUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brobot.Models.HotOpModel", b =>
                {
                    b.HasOne("Brobot.Models.ChannelModel", "Channel")
                        .WithMany("HotOps")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brobot.Models.UserModel", "User")
                        .WithMany("HotOps")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brobot.Models.HotOpSessionModel", b =>
                {
                    b.HasOne("Brobot.Models.HotOpModel", "HotOp")
                        .WithMany("HotOpSessions")
                        .HasForeignKey("HotOpId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brobot.Models.UserModel", "User")
                        .WithMany("HotOpSessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HotOp");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brobot.Models.PlaylistModel", b =>
                {
                    b.HasOne("Brobot.Models.UserModel", "User")
                        .WithMany("Playlists")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brobot.Models.PlaylistSongModel", b =>
                {
                    b.HasOne("Brobot.Models.PlaylistModel", "Playlist")
                        .WithMany("Songs")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Playlist");
                });

            modelBuilder.Entity("Brobot.Models.ScheduledMessageModel", b =>
                {
                    b.HasOne("Brobot.Models.ChannelModel", "Channel")
                        .WithMany("ScheduledMessages")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brobot.Models.UserModel", "CreatedBy")
                        .WithMany("ScheduledMessages")
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("Brobot.Models.UserModel", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", "IdentityUser")
                        .WithMany()
                        .HasForeignKey("IdentityUserId");

                    b.HasOne("Brobot.Models.ChannelModel", "PrimaryChannel")
                        .WithMany("Users")
                        .HasForeignKey("PrimaryChannelId");

                    b.Navigation("IdentityUser");

                    b.Navigation("PrimaryChannel");
                });

            modelBuilder.Entity("Brobot.Models.ChannelModel", b =>
                {
                    b.Navigation("ChannelUsers");

                    b.Navigation("DailyMessageCounts");

                    b.Navigation("HotOps");

                    b.Navigation("ScheduledMessages");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Brobot.Models.GuildModel", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("GuildUsers");
                });

            modelBuilder.Entity("Brobot.Models.HotOpModel", b =>
                {
                    b.Navigation("HotOpSessions");
                });

            modelBuilder.Entity("Brobot.Models.PlaylistModel", b =>
                {
                    b.Navigation("Songs");
                });

            modelBuilder.Entity("Brobot.Models.UserModel", b =>
                {
                    b.Navigation("ChannelUsers");

                    b.Navigation("DailyCounts");

                    b.Navigation("GuildUsers");

                    b.Navigation("HotOpSessions");

                    b.Navigation("HotOps");

                    b.Navigation("Playlists");

                    b.Navigation("ScheduledMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
