using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Contexts;

public class BrobotDbContext : DbContext
{
    public DbSet<GuildModel> Guilds => Set<GuildModel>();
    public DbSet<ChannelModel> Channels => Set<ChannelModel>();
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<ScheduledMessageModel> ScheduledMessages => Set<ScheduledMessageModel>();
    public DbSet<HotOpModel> HotOps => Set<HotOpModel>();
    public DbSet<DailyMessageCountModel> DailyMessageCounts => Set<DailyMessageCountModel>();
    public DbSet<PlaylistModel> Playlists => Set<PlaylistModel>();
    public DbSet<PlaylistSongModel> PlaylistSongs => Set<PlaylistSongModel>();
    public DbSet<SecretSantaGroupModel> SecretSantaGroups => Set<SecretSantaGroupModel>();
    public DbSet<SecretSantaPairModel> SecretSantaPairs => Set<SecretSantaPairModel>();
    public DbSet<StopWordModel> StopWords => Set<StopWordModel>();

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
            .IsRequired()
            .HasMaxLength(255);
        builder.Entity<GuildModel>()
            .Property((g) => g.Archived)
            .HasColumnName("archived")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Entity<ChannelModel>()
            .ToTable(name: "channel", schema: "brobot")
            .HasKey((c) => c.Id);
        builder.Entity<ChannelModel>()
            .Property((c) => c.Id)
            .HasColumnName("id");
        builder.Entity<ChannelModel>()
            .Property((c) => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);
        builder.Entity<ChannelModel>()
            .Property((c) => c.Archived)
            .HasColumnName("archived")
            .HasDefaultValue(false)
            .IsRequired();
        builder.Entity<ChannelModel>()
            .Property((c) => c.MonthlyWordCloud)
            .HasColumnName("monthly_word_cloud")
            .IsRequired()
            .HasDefaultValue(false);
        builder.Entity<ChannelModel>()
            .Property((c) => c.GuildId)
            .HasColumnName("guild_id")
            .IsRequired();
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
            .IsRequired()
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
            .IsRequired();
        builder.Entity<UserModel>()
            .Property((u) => u.PrimaryChannelId)
            .HasColumnName("primary_channel_id")
            .IsRequired(false)
            .HasDefaultValue(null);
        builder.Entity<UserModel>()
            .Property((u) => u.LastOnline)
            .HasColumnName("last_online")
            .IsRequired(false);
        builder.Entity<UserModel>()
            .HasOne((u) => u.PrimaryChannel)
            .WithMany((c) => c.Users)
            .HasForeignKey((u) => u.PrimaryChannelId);
        builder.Entity<UserModel>()
            .Property((u) => u.IdentityUserId)
            .HasColumnName("identity_user_id")
            .IsRequired(false);
        builder.Entity<UserModel>()
            .HasIndex((u) => u.IdentityUserId)
            .IsUnique();
        builder.Entity<UserModel>()
            .HasOne((u) => u.IdentityUser)
            .WithMany()
            .HasForeignKey((u) => u.IdentityUserId);

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
            .IsRequired();
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();
        builder.Entity<ScheduledMessageModel>()
            .Property((sm) => sm.MessageText)
            .HasColumnName("message_text")
            .HasMaxLength(1000)
            .IsRequired();
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

        builder.Entity<HotOpModel>()
            .ToTable(name: "hot_op", schema: "brobot")
            .HasKey((ho) => ho.Id);
        builder.Entity<HotOpModel>()
            .Property((ho) => ho.Id)
            .HasColumnName("id");
        builder.Entity<HotOpModel>()
            .Property((ho) => ho.ChannelId)
            .HasColumnName("channel_id")
            .IsRequired();
        builder.Entity<HotOpModel>()
            .Property((ho) => ho.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        builder.Entity<HotOpModel>()
            .Property((ho) => ho.StartDate)
            .HasColumnName("start_date")
            .IsRequired();
        builder.Entity<HotOpModel>()
            .Property((ho) => ho.EndDate)
            .HasColumnName("end_date")
            .IsRequired();
        builder.Entity<HotOpModel>()
            .HasOne((ho) => ho.Channel)
            .WithMany((c) => c.HotOps)
            .HasForeignKey((ho) => ho.ChannelId);
        builder.Entity<HotOpModel>()
            .HasOne((ho) => ho.User)
            .WithMany((u) => u.HotOps)
            .HasForeignKey((ho) => ho.UserId);

        builder.Entity<HotOpSessionModel>()
            .ToTable(name: "hot_op_session", schema: "brobot")
            .HasKey((hos) => hos.Id);
        builder.Entity<HotOpSessionModel>()
            .Property((hos) => hos.Id)
            .HasColumnName("id");
        builder.Entity<HotOpSessionModel>()
            .Property((hos) => hos.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        builder.Entity<HotOpSessionModel>()
            .Property((hos) => hos.StartDateTime)
            .HasColumnName("start_date_time")
            .IsRequired();
        builder.Entity<HotOpSessionModel>()
            .Property((hos) => hos.EndDateTime)
            .HasColumnName("end_date_time")
            .IsRequired(false);
        builder.Entity<HotOpSessionModel>()
            .Property((hos) => hos.HotOpId)
            .HasColumnName("hot_op_id")
            .IsRequired();
        builder.Entity<HotOpSessionModel>()
            .HasOne((hos) => hos.User)
            .WithMany((hos) => hos.HotOpSessions)
            .HasForeignKey((ho) => ho.UserId);
        builder.Entity<HotOpSessionModel>()
            .HasOne((hos) => hos.HotOp)
            .WithMany((ho) => ho.HotOpSessions)
            .HasForeignKey((hos) => hos.HotOpId);

        builder.Entity<DailyMessageCountModel>()
            .ToTable(name: "daily_message_count", schema: "brobot")
            .HasKey((dc) => new { dc.UserId, dc.ChannelId, dc.CountDate });
        builder.Entity<DailyMessageCountModel>()
            .Property((dc) => dc.CountDate)
            .HasColumnName("count_date");
        builder.Entity<DailyMessageCountModel>()
            .Property((dc) => dc.UserId)
            .HasColumnName("user_id");
        builder.Entity<DailyMessageCountModel>()
            .Property((dc) => dc.MessageCount)
            .HasColumnName("message_count")
            .HasDefaultValue(0);
        builder.Entity<DailyMessageCountModel>()
            .Property((dc) => dc.ChannelId)
            .HasColumnName("channel_id");
        builder.Entity<DailyMessageCountModel>()
            .HasOne((dc) => dc.User)
            .WithMany((u) => u.DailyCounts)
            .HasForeignKey((dc) => dc.UserId);
        builder.Entity<DailyMessageCountModel>()
            .HasOne((dc) => dc.Channel)
            .WithMany((c) => c.DailyMessageCounts)
            .HasForeignKey((dc) => dc.ChannelId);

        builder.Entity<PlaylistModel>()
            .ToTable(name: "playlist", schema: "brobot")
            .HasKey((p) => p.Id);
        builder.Entity<PlaylistModel>()
            .Property((p) => p.Id)
            .HasColumnName("id");
        builder.Entity<PlaylistModel>()
            .Property((p) => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(256);
        builder.Entity<PlaylistModel>()
            .Property((p) => p.UserId)
            .HasColumnName("discord_user_id")
            .IsRequired();
        builder.Entity<PlaylistModel>()
            .HasOne((p) => p.User)
            .WithMany((u) => u.Playlists)
            .HasForeignKey((p) => p.UserId);

        builder.Entity<PlaylistSongModel>()
            .ToTable(name: "playlist_song", schema: "brobot")
            .HasKey((ps) => ps.Id);
        builder.Entity<PlaylistSongModel>()
            .Property((ps) => ps.Id)
            .HasColumnName("id");
        builder.Entity<PlaylistSongModel>()
            .Property((ps) => ps.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(256);
        builder.Entity<PlaylistSongModel>()
            .Property((ps) => ps.Artist)
            .HasColumnName("artist")
            .IsRequired()
            .HasMaxLength(256);
        builder.Entity<PlaylistSongModel>()
            .Property((ps) => ps.Url)
            .HasColumnName("url")
            .IsRequired()
            .HasMaxLength(1024);
        builder.Entity<PlaylistSongModel>()
            .Property((ps) => ps.PlaylistId)
            .HasColumnName("playlist_id")
            .IsRequired();
        builder.Entity<PlaylistSongModel>()
            .Property((ps) => ps.Order)
            .HasColumnName("order")
            .IsRequired();
        builder.Entity<PlaylistSongModel>()
            .HasOne((ps) => ps.Playlist)
            .WithMany((p) => p.Songs)
            .HasForeignKey((ps) => ps.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<PlaylistSongModel>()
            .HasIndex((ps) => new { ps.PlaylistId, ps.Order })
            .IsUnique();

        builder.Entity<SecretSantaGroupModel>()
            .ToTable(name: "secret_santa_group", schema: "brobot")
            .HasKey((ssg) => ssg.Id);
        builder.Entity<SecretSantaGroupModel>()
            .Property((ssg) => ssg.Id)
            .HasColumnName("id")
            .IsRequired();
        builder.Entity<SecretSantaGroupModel>()
            .Property((ssg) => ssg.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Entity<SecretSantaGroupUserModel>()
            .ToTable(name: "secret_santa_group_user", schema: "brobot")
            .HasKey((ssgu) => new { ssgu.UserId, ssgu.SecretSantaGroupId });
        builder.Entity<SecretSantaGroupUserModel>()
            .Property((ssgu) => ssgu.SecretSantaGroupId)
            .HasColumnName("secret_santa_group_id");
        builder.Entity<SecretSantaGroupUserModel>()
            .Property((ssgu) => ssgu.UserId)
            .HasColumnName("user_id");
        builder.Entity<SecretSantaGroupUserModel>()
            .HasOne((ssgu) => ssgu.SecretSantaGroup)
            .WithMany((ssg) => ssg.SecretSantaGroupUsers)
            .HasForeignKey((ssg) => ssg.SecretSantaGroupId);
        builder.Entity<SecretSantaGroupUserModel>()
            .HasOne((ssgu) => ssgu.User)
            .WithMany((u) => u.SecretSantaGroupUsers)
            .HasForeignKey((ssgu) => ssgu.UserId);

        builder.Entity<SecretSantaPairModel>()
            .ToTable(name: "secret_santa_pair", schema: "brobot")
            .HasKey((ssp) => ssp.Id);
        builder.Entity<SecretSantaPairModel>()
            .Property((ssp) => ssp.Id)
            .HasColumnName("id");
        builder.Entity<SecretSantaPairModel>()
            .Property((ssp) => ssp.SecretSantaGroupId)
            .HasColumnName("secret_santa_group_id")
            .IsRequired();
        builder.Entity<SecretSantaPairModel>()
            .Property((ssp) => ssp.Year)
            .HasColumnName("year")
            .IsRequired();
        builder.Entity<SecretSantaPairModel>()
            .Property((ssp) => ssp.GiverUserId)
            .HasColumnName("giver_user_id")
            .IsRequired();
        builder.Entity<SecretSantaPairModel>()
            .Property((ssp) => ssp.RecipientUserId)
            .HasColumnName("recipient_user_id")
            .IsRequired();
        builder.Entity<SecretSantaPairModel>()
            .HasOne((ssp) => ssp.SecretSantaGroup)
            .WithMany((ssg) => ssg.SecretSantaPairs)
            .HasForeignKey((ssp) => ssp.SecretSantaGroupId);
        builder.Entity<SecretSantaPairModel>()
            .HasOne((ssp) => ssp.GiverUser)
            .WithMany((u) => u.Givers)
            .HasForeignKey((ssp) => ssp.GiverUserId);
        builder.Entity<SecretSantaPairModel>()
            .HasOne((ssp) => ssp.RecipientUser)
            .WithMany((u) => u.Recipients)
            .HasForeignKey((ssp) => ssp.RecipientUserId);

        builder.Entity<StopWordModel>()
            .ToTable(name: "stop_word", schema: "brobot")
            .HasKey((sw) => sw.Id);
        builder.Entity<StopWordModel>()
            .Property((sw) => sw.Id)
            .HasColumnName("id");
        builder.Entity<StopWordModel>()
            .Property((sw) => sw.Word)
            .HasColumnName("word")
            .IsRequired();
        builder.Entity<StopWordModel>()
            .HasIndex((sw) => sw.Word)
            .IsUnique();
    }
}