﻿// <auto-generated />
using System;
using Brobot.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Brobot.Migrations
{
    [DbContext(typeof(BrobotDbContext))]
    partial class BrobotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
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

                    b.Property<DateTime?>("SendDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("send_date");

                    b.Property<DateTime?>("SentDate")
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

                    b.Property<DateTime?>("Birthdate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("birthdate");

                    b.Property<DateTime?>("LastOnline")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_online");

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

                    b.ToTable("discord_user", "brobot");
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

            modelBuilder.Entity("Brobot.Models.ChannelModel", b =>
                {
                    b.Navigation("ChannelUsers");

                    b.Navigation("ScheduledMessages");
                });

            modelBuilder.Entity("Brobot.Models.GuildModel", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("GuildUsers");
                });

            modelBuilder.Entity("Brobot.Models.UserModel", b =>
                {
                    b.Navigation("ChannelUsers");

                    b.Navigation("GuildUsers");

                    b.Navigation("ScheduledMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
