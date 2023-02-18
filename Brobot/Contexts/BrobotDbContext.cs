using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Contexts;

public class BrobotDbContext : DbContext
{
    public DbSet<GuildModel> Guilds => Set<GuildModel>();
    public DbSet<ChannelModel> Channels => Set<ChannelModel>();
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<ScheduledMessageModel> ScheduledMessages => Set<ScheduledMessageModel>();

    public BrobotDbContext(DbContextOptions<BrobotDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<GuildModel>()
            .ToTable(name: "guild", schema: "brobot")
            .HasKey((g) => g.Id);
        builder.Entity<GuildModel>()
            .Property((g) => g.Id)
            .HasColumnName("id");
        builder.Entity<GuildModel>()
            .Property((g) => g.Name)
            .HasColumnName("name")
            .IsRequired(true)
            .HasMaxLength(255);
        builder.Entity<GuildModel>()
            .Property((g) => g.Archived)
            .HasColumnName("archived")
            .HasDefaultValue(false)
            .IsRequired(true);

        builder.Entity<ChannelModel>()
            .ToTable(name: "channel", schema: "brobot")
            .HasKey((c) => c.Id);
        builder.Entity<ChannelModel>()
            .Property((c) => c.Id)
            .HasColumnName("id");
        builder.Entity<ChannelModel>()
            .Property((c) => c.Name)
            .HasColumnName("name")
            .IsRequired(true)
            .HasMaxLength(255);
        builder.Entity<ChannelModel>()
            .Property((c) => c.Archived)
            .HasColumnName("archived")
            .HasDefaultValue(false)
            .IsRequired(true);
        builder.Entity<ChannelModel>()
            .Property((c) => c.GuildId)
            .HasColumnName("guild_id")
            .IsRequired(true);
        builder.Entity<ChannelModel>()
            .HasOne((c) => c.Guild)
            .WithMany((g) => g.Channels)
            .HasForeignKey((c) => c.GuildId);

        builder.Entity<UserModel>()
            .ToTable(name: "discord_user", schema: "brobot")
            .HasKey((u) => u.Id);
        builder.Entity<UserModel>()
            .Property((u) => u.Id)
            .HasColumnName("id");
        builder.Entity<UserModel>()
            .Property((u) => u.Username)
            .HasColumnName("username")
            .IsRequired(true)
            .HasMaxLength(255);
        builder.Entity<UserModel>()
            .Property((u) => u.Birthdate)
            .HasColumnName("birthdate")
            .IsRequired(false);
        builder.Entity<UserModel>()
            .Property((u) => u.Timezone)
            .HasColumnName("timezone")
            .IsRequired(false)
            .HasMaxLength(255);
        builder.Entity<UserModel>()
            .Property((u) => u.Archived)
            .HasColumnName("archived")
            .HasDefaultValue(false)
            .IsRequired(true);
        builder.Entity<UserModel>()
            .Property((u) => u.LastOnline)
            .HasColumnName("last_online")
            .IsRequired(false);

        builder.Entity<GuildUserModel>()
            .ToTable(name: "guild_user", schema: "brobot")
            .HasKey((gu) => new { gu.GuildId, gu.UserId });
        builder.Entity<GuildUserModel>()
            .Property((gu) => gu.GuildId)
            .HasColumnName("guild_id");
        builder.Entity<GuildUserModel>()
            .Property((gu) => gu.UserId)
            .HasColumnName("user_id");
        builder.Entity<GuildUserModel>()
            .HasOne((gu) => gu.Guild)
            .WithMany((g) => g.GuildUsers)
            .HasForeignKey((gu) => gu.GuildId);
        builder.Entity<GuildUserModel>()
            .HasOne((gu) => gu.User)
            .WithMany((u) => u.GuildUsers)
            .HasForeignKey((gu) => gu.UserId);

        builder.Entity<ChannelUserModel>()
            .ToTable(name: "channel_user", schema: "brobot")
            .HasKey((cu) => new { cu.ChannelId, cu.UserId });
        builder.Entity<ChannelUserModel>()
            .Property((cu) => cu.ChannelId)
            .HasColumnName("channel_id");
        builder.Entity<ChannelUserModel>()
            .Property((cu) => cu.UserId)
            .HasColumnName("user_id");
        builder.Entity<ChannelUserModel>()
            .HasOne((cu) => cu.Channel)
            .WithMany((g) => g.ChannelUsers)
            .HasForeignKey((cu) => cu.ChannelId);
        builder.Entity<ChannelUserModel>()
            .HasOne((cu) => cu.User)
            .WithMany((u) => u.ChannelUsers)
            .HasForeignKey((cu) => cu.UserId);

        builder.Entity<ScheduledMessageModel>()
            .ToTable(name: "scheduled_message", schema: "brobot")
            .HasKey((sm) => sm.Id);
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.Id)
            .HasColumnName("id");
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.ChannelId)
            .HasColumnName("channel_id")
            .IsRequired(true);
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired(true);
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.MessageText)
            .HasColumnName("message_text")
            .HasMaxLength(1000)
            .IsRequired(true);
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.SendDate)
            .HasColumnName("send_date")
            .IsRequired(false);
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.SentDate)
            .HasColumnName("sent_date")
            .IsRequired(false);
        builder.Entity<ScheduledMessageModel>()
            .HasOne((sm) => sm.Channel)
            .WithMany((c) => c.ScheduledMessages)
            .HasForeignKey((sm) => sm.ChannelId);
        builder.Entity<ScheduledMessageModel>()
            .HasOne((sm) => sm.CreatedBy)
            .WithMany((u) => u.ScheduledMessages)
            .HasForeignKey((sm) => sm.CreatedById);
    }
}