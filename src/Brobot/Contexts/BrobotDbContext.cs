using Brobot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Contexts;

public class BrobotDbContext(DbContextOptions<BrobotDbContext> options) : IdentityDbContext<ApplicationUserModel>(options)
{
    public DbSet<GuildModel> Guilds => Set<GuildModel>();
    public DbSet<ChannelModel> Channels => Set<ChannelModel>();
    public DbSet<DiscordUserModel> DiscordUsers => Set<DiscordUserModel>();
    public DbSet<ScheduledMessageModel> ScheduledMessages => Set<ScheduledMessageModel>();
    public DbSet<HotOpModel> HotOps => Set<HotOpModel>();
    public DbSet<DailyMessageCountModel> DailyMessageCounts => Set<DailyMessageCountModel>();
    public DbSet<SecretSantaGroupModel> SecretSantaGroups => Set<SecretSantaGroupModel>();
    public DbSet<SecretSantaPairModel> SecretSantaPairs => Set<SecretSantaPairModel>();
    public DbSet<StopWordModel> StopWords => Set<StopWordModel>();
    public DbSet<VersionModel> Versions => Set<VersionModel>();
    public DbSet<StatPeriodModel> StatPeriods => Set<StatPeriodModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<GuildModel>(entity =>
        {
            entity.ToTable(name: "guilds", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Archived)
                .HasColumnName("archived")
                .HasDefaultValue(false)
                .IsRequired();
            entity.Property(e => e.PrimaryChannelId)
                .HasColumnName("primary_channel_id");
            
            entity.HasOne(e => e.PrimaryChannel)
                .WithOne()
                .HasForeignKey<GuildModel>(e => e.PrimaryChannelId);
        });

        builder.Entity<ChannelModel>(entity =>
        {
            entity.ToTable(name: "channels", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Archived)
                .HasColumnName("archived")
                .HasDefaultValue(false)
                .IsRequired();
            entity.Property(e => e.MonthlyWordCloud)
                .HasColumnName("monthly_word_cloud")
                .IsRequired()
                .HasDefaultValue(false);
            entity.Property(e => e.GuildId)
                .HasColumnName("guild_id")
                .IsRequired();
            entity.Property(e => e.Timezone)
                .HasColumnName("timezone")
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValue("america/chicago");
            
            entity.HasOne(e => e.Guild)
                .WithMany(g => g.Channels)
                .HasForeignKey(e => e.GuildId);
        });

        builder.Entity<DiscordUserModel>(entity =>
        {
            entity.ToTable(name: "discord_users", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Birthdate)
                .HasColumnName("birthdate")
                .IsRequired(false);
            entity.Property(e => e.Timezone)
                .HasColumnName("timezone")
                .IsRequired(false)
                .HasMaxLength(255);
            entity.Property(e => e.Archived)
                .HasColumnName("archived")
                .HasDefaultValue(false)
                .IsRequired();
            entity.Property(e => e.PrimaryChannelId)
                .HasColumnName("primary_channel_id")
                .IsRequired(false)
                .HasDefaultValue(null);
            entity.Property(e => e.LastOnline)
                .HasColumnName("last_online")
                .IsRequired(false);
            
            entity.HasOne(e => e.PrimaryChannel)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.PrimaryChannelId);
        });

        builder.Entity<GuildDiscordUserModel>(entity =>
        {
            entity.ToTable(name: "guild_discord_users", schema: "brobot");
            entity.HasKey(e => new { e.GuildId, UserId = e.DiscordUserId });
            
            entity.Property(e => e.GuildId)
                .HasColumnName("guild_id");
            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id");
            
            entity.HasOne(e => e.Guild)
                .WithMany(g => g.GuildUsers)
                .HasForeignKey(e => e.GuildId);
            entity.HasOne(e => e.DiscordUser)
                .WithMany(u => u.GuildUsers)
                .HasForeignKey(e => e.DiscordUserId);
        });

        builder.Entity<ChannelDiscordUserModel>(entity =>
        {
            entity.ToTable(name: "channel_discord_users", schema: "brobot");
            entity.HasKey(e => new { e.ChannelId, e.UserId });
            
            entity.Property(e => e.ChannelId)
                .HasColumnName("channel_id");
            entity.Property(e => e.UserId)
                .HasColumnName("discord_user_id");
            
            entity.HasOne(e => e.Channel)
                .WithMany(c => c.ChannelUsers)
                .HasForeignKey(e => e.ChannelId);
            entity.HasOne(e => e.DiscordUser)
                .WithMany(u => u.ChannelUsers)
                .HasForeignKey(e => e.UserId);
        });

        builder.Entity<ScheduledMessageModel>(entity =>
        {
            entity.ToTable(name: "scheduled_messages", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.ChannelId)
                .HasColumnName("channel_id")
                .IsRequired();
            entity.Property(e => e.CreatedById)
                .HasColumnName("created_by_id")
                .IsRequired();
            entity.Property(e => e.MessageText)
                .HasColumnName("message_text")
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(e => e.SendDate)
                .HasColumnName("send_date")
                .IsRequired(false);
            entity.Property(e => e.SentDate)
                .HasColumnName("sent_date")
                .IsRequired(false);
            
            entity.HasOne(e => e.Channel)
                .WithMany(c => c.ScheduledMessages)
                .HasForeignKey(e => e.ChannelId);
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.ScheduledMessages)
                .HasForeignKey(e => e.CreatedById);
        });

        builder.Entity<HotOpModel>(entity =>
        {
            entity.ToTable(name: "hot_ops", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.ChannelId)
                .HasColumnName("channel_id")
                .IsRequired();
            entity.Property(e => e.UserId)
                .HasColumnName("discord_user_id")
                .IsRequired();
            entity.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .IsRequired();
            entity.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .IsRequired();
            
            entity.HasOne(e => e.Channel)
                .WithMany(c => c.HotOps)
                .HasForeignKey(e => e.ChannelId);
            entity.HasOne(e => e.DiscordUser)
                .WithMany(u => u.HotOps)
                .HasForeignKey(e => e.UserId);
        });

        builder.Entity<HotOpSessionModel>(entity =>
        {
            entity.ToTable(name: "hot_op_sessions", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .IsRequired();
            entity.Property(e => e.StartDateTime)
                .HasColumnName("start_date_time")
                .IsRequired();
            entity.Property(e => e.EndDateTime)
                .HasColumnName("end_date_time")
                .IsRequired(false);
            entity.Property(e => e.HotOpId)
                .HasColumnName("hot_op_id")
                .IsRequired();
            
            entity.HasOne(e => e.DiscordUser)
                .WithMany(u => u.HotOpSessions)
                .HasForeignKey(e => e.DiscordUserId);
            entity.HasOne(e => e.HotOp)
                .WithMany(h => h.HotOpSessions)
                .HasForeignKey(e => e.HotOpId);
        });

        builder.Entity<DailyMessageCountModel>(entity =>
        {
            entity.ToTable(name: "daily_message_counts", schema: "brobot");
            entity.HasKey(e => new { e.DiscordUserId, e.ChannelId, e.CountDate });
            
            entity.Property(e => e.CountDate)
                .HasColumnName("count_date");
            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id");
            entity.Property(e => e.MessageCount)
                .HasColumnName("message_count")
                .HasDefaultValue(0);
            entity.Property(e => e.ChannelId)
                .HasColumnName("channel_id");
            
            entity.HasOne(e => e.DiscordUser)
                .WithMany(u => u.DailyCounts)
                .HasForeignKey(e => e.DiscordUserId);
            entity.HasOne(e => e.Channel)
                .WithMany(c => c.DailyMessageCounts)
                .HasForeignKey(e => e.ChannelId);
        });

        builder.Entity<SecretSantaGroupModel>(entity =>
        {
            entity.ToTable(name: "secret_santa_groups", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.Entity<SecretSantaGroupDiscordUserModel>(entity =>
        {
            entity.ToTable(name: "secret_santa_group_users", schema: "brobot");
            entity.HasKey(e => new { e.DiscordUserId, e.SecretSantaGroupId });
            
            entity.Property(e => e.SecretSantaGroupId)
                .HasColumnName("secret_santa_group_id");
            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id");
            
            entity.HasOne(e => e.SecretSantaGroup)
                .WithMany(g => g.SecretSantaGroupUsers)
                .HasForeignKey(e => e.SecretSantaGroupId);
            entity.HasOne(e => e.DiscordUser)
                .WithMany(u => u.SecretSantaGroupUsers)
                .HasForeignKey(e => e.DiscordUserId);
        });

        builder.Entity<SecretSantaPairModel>(entity =>
        {
            entity.ToTable(name: "secret_santa_pairs", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.SecretSantaGroupId)
                .HasColumnName("secret_santa_group_id")
                .IsRequired();
            entity.Property(e => e.Year)
                .HasColumnName("year")
                .IsRequired();
            entity.Property(e => e.GiverDiscordUserId)
                .HasColumnName("giver_discord_user_id")
                .IsRequired();
            entity.Property(e => e.RecipientDiscordUserId)
                .HasColumnName("recipient_discord_user_id")
                .IsRequired();
            
            entity.HasOne(e => e.SecretSantaGroup)
                .WithMany(g => g.SecretSantaPairs)
                .HasForeignKey(e => e.SecretSantaGroupId);
            entity.HasOne(e => e.GiverDiscordUser)
                .WithMany(u => u.Givers)
                .HasForeignKey(e => e.GiverDiscordUserId);
            entity.HasOne(e => e.RecipientDiscordUser)
                .WithMany(u => u.Recipients)
                .HasForeignKey(e => e.RecipientDiscordUserId);
        });

        builder.Entity<StopWordModel>(entity =>
        {
            entity.ToTable(name: "stop_words", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Word)
                .HasColumnName("word")
                .HasMaxLength(255)
                .IsRequired();
            
            entity.HasIndex(e => e.Word)
                .IsUnique();
        });

        builder.Entity<VersionModel>(entity =>
        {
            entity.ToTable(name: "versions", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.VersionDate)
                .HasColumnName("version_date")
                .IsRequired()
                .HasDefaultValueSql("NOW()");
        });

        builder.Entity<StatPeriodModel>(entity =>
        {
            entity.ToTable(name: "stat_periods", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.ChannelId)
                .HasColumnName("channel_id");
            entity.Property(e => e.StartDate)
                .HasColumnName("start_date");
            entity.Property(e => e.EndDate)
                .HasColumnName("end_date");
            
            entity.HasOne(e => e.Channel)
                .WithMany(c => c.StatPeriods)
                .HasForeignKey(e => e.ChannelId);
        });

        builder.Entity<WordCountModel>(entity =>
        {
            entity.ToTable(name: "word_counts", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Word)
                .HasColumnName("word")
                .HasMaxLength(255);
            entity.Property(e => e.Count)
                .HasColumnName("count");
            
            entity.HasOne(e => e.StatPeriod)
                .WithMany(s => s.WordCounts)
                .HasForeignKey(e => e.StatPeriodId);
        });

        builder.Entity<DiscordUserMessageCountModel>(entity =>
        {
            entity.ToTable(name: "user_message_counts", schema: "brobot");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id");
            entity.Property(e => e.Count)
                .HasColumnName("count");
            entity.Property(e => e.StatPeriodId)
                .HasColumnName("stat_period_id");
            
            entity.HasOne(e => e.StatPeriod)
                .WithMany(s => s.UserMessageCounts)
                .HasForeignKey(e => e.StatPeriodId);
            entity.HasOne<DiscordUserModel>()
                .WithMany()
                .HasForeignKey(e => e.DiscordUserId);
        });
        
        // Configure UserModel columns to use snake_case
        builder.Entity<ApplicationUserModel>(entity =>
        {
            entity.ToTable("users");
            
            // IdentityUser properties
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.UserName)
                .HasColumnName("user_name");
            entity.Property(e => e.NormalizedUserName)
                .HasColumnName("normalized_user_name");
            entity.Property(e => e.Email)
                .HasColumnName("email");
            entity.Property(e => e.NormalizedEmail)
                .HasColumnName("normalized_email");
            entity.Property(e => e.EmailConfirmed)
                .HasColumnName("email_confirmed");
            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash");
            entity.Property(e => e.SecurityStamp)
                .HasColumnName("security_stamp");
            entity.Property(e => e.ConcurrencyStamp)
                .HasColumnName("concurrency_stamp");
            entity.Property(e => e.PhoneNumber)
                .HasColumnName("phone_number");
            entity.Property(e => e.PhoneNumberConfirmed)
                .HasColumnName("phone_number_confirmed");
            entity.Property(e => e.TwoFactorEnabled)
                .HasColumnName("two_factor_enabled");
            entity.Property(e => e.LockoutEnd)
                .HasColumnName("lockout_end");
            entity.Property(e => e.LockoutEnabled)
                .HasColumnName("lockout_enabled");
            entity.Property(e => e.AccessFailedCount)
                .HasColumnName("access_failed_count");
            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id");
            
            entity.HasOne(e => e.DiscordUser)
                .WithMany()
                .HasForeignKey(e => e.DiscordUserId);
        });

        // Configure IdentityRole columns to use snake_case
        builder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("roles");
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnName("name");
            entity.Property(e => e.NormalizedName)
                .HasColumnName("normalized_name");
            entity.Property(e => e.ConcurrencyStamp)
                .HasColumnName("concurrency_stamp");
        });

        // Configure IdentityUserRole columns to use snake_case
        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("user_roles");
            
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");
        });

        // Configure IdentityUserClaim columns to use snake_case
        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("user_claims");
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.ClaimType)
                .HasColumnName("claim_type");
            entity.Property(e => e.ClaimValue)
                .HasColumnName("claim_value");
        });

        // Configure IdentityUserLogin columns to use snake_case
        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("user_logins");
            
            entity.Property(e => e.LoginProvider)
                .HasColumnName("login_provider");
            entity.Property(e => e.ProviderKey)
                .HasColumnName("provider_key");
            entity.Property(e => e.ProviderDisplayName)
                .HasColumnName("provider_display_name");
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
        });

        // Configure IdentityUserToken columns to use snake_case
        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("user_tokens");
            
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.LoginProvider)
                .HasColumnName("login_provider");
            entity.Property(e => e.Name)
                .HasColumnName("name");
            entity.Property(e => e.Value)
                .HasColumnName("value");
        });

        // Configure IdentityRoleClaim columns to use snake_case
        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("role_claims");
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");
            entity.Property(e => e.ClaimType)
                .HasColumnName("claim_type");
            entity.Property(e => e.ClaimValue)
                .HasColumnName("claim_value");
        });
    }
}