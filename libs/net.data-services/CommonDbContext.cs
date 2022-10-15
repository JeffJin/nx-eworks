using System;
using System.Diagnostics;
using adworks.data_services.DbModels;
using adworks.data_services.Identity;
using adworks.media_common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class CommonDbContext: IdentityDbContext<User, Role,
        string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        private readonly ILogger? _logger;

        public CommonDbContext()
        {

        }

        public CommonDbContext(ILogger logger)
        {
            _logger = logger;
        }

        public CommonDbContext(ILogger logger, DbContextOptions<CommonDbContext> options)
            : base(options)
        {
            _logger = logger;
        }

        //Application models
        public virtual DbSet<Video> Videos { get; set; }
        public virtual DbSet<Audio> Audios { get; set; }
        public virtual DbSet<Image> Images { get; set; }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<SubCategory> SubCategories { get; set; }
        public virtual DbSet<Record> Records { get; set; }

        //appointments
        public virtual DbSet<Appointment> Appointments { get; set; }

        //shopping entities
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }

        //Organizations
        public virtual DbSet<Organization> Organizations { get; set; }

        public virtual DbSet<License> Licenses { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DeviceStatus> DeviceStatuses { get; set; }
        public virtual DbSet<DeviceGroup> DeviceGroups { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<PlaylistGroup> PlaylistGroups { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<SubPlaylist> SubPlaylists { get; set; }
        public virtual DbSet<PlaylistItem> PlaylistItems { get; set; }
        public virtual DbSet<MergeRecord> MergeRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = AppConfig.GetConfig();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            _logger?.Information(
                $"CommonDbContext uses the following connection string to connect to mysql server :: {connectionString}");
            optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 23)),
                    mySqlOptions => mySqlOptions
                        .EnableRetryOnFailure())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Asp Net Identity Tables
            modelBuilder.Entity<User>()
                .HasAlternateKey(c => c.Email)
                .HasName("AK_Email");

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(500);
            modelBuilder.Entity<User>().Property(u => u.ConcurrencyStamp).HasMaxLength(500);
            modelBuilder.Entity<User>().Property(u => u.PhoneNumber).HasMaxLength(50);

            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<UserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<RoleClaim>().ToTable("RoleClaims");
            modelBuilder.Entity<UserToken>().ToTable("UserTokens");
            modelBuilder.Entity<UserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<UserClaim>().Property(u => u.ClaimType).HasMaxLength(150);
            modelBuilder.Entity<UserClaim>().Property(u => u.ClaimValue).HasMaxLength(500);


            //organizations
            modelBuilder.Entity<Organization>().ToTable("Organizations");
            modelBuilder.Entity<Organization>()
                .HasIndex(b => b.Name)
                .IsUnique();

            //category
            modelBuilder.Entity<Category>()
                .HasIndex(b => b.Name)
                .IsUnique();
            modelBuilder.Entity<SubCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_category_id");

            //order
            modelBuilder.Entity<Product>()
                .HasIndex(b => b.Title)
                .IsUnique();
            modelBuilder.Entity<Payment>()
                .HasIndex(b => b.TransactionId)
                .IsUnique();
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(c => c.OrderItems)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_order_id");

            //video
            modelBuilder.Entity<Video>()
                .HasIndex(b => b.ThumbnailLink)
                .IsUnique();
            modelBuilder.Entity<Video>()
                .HasIndex(b => b.VodVideoUrl)
                .IsUnique();
            modelBuilder.Entity<Video>()
                .HasIndex(c => c.RawFilePath)
                .IsUnique();

            //audio
            modelBuilder.Entity<Audio>()
                .HasAlternateKey(c => c.CloudUrl)
                .HasName("AK_Audio_CloudUrl");
            modelBuilder.Entity<Audio>()
                .HasIndex(c => c.EncodedFilePath)
                .IsUnique();
            modelBuilder.Entity<Audio>()
                .HasIndex(c => c.RawFilePath)
                .IsUnique();

            //image
            modelBuilder.Entity<Image>()
                .HasIndex(c => c.CloudUrl)
                .IsUnique();
            modelBuilder.Entity<Image>()
                .HasIndex(c => c.RawFilePath)
                .IsUnique();

            //device
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Location)
                .WithMany(l => l.Devices)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.DeviceGroup)
                .WithMany(l => l.Devices)
                .OnDelete(DeleteBehavior.NoAction);

            //device
            modelBuilder.Entity<Device>()
                .HasIndex(c => c.SerialNumber)
                .IsUnique();

            //device
            modelBuilder.Entity<License>()
                .HasOne(l => l.Device)
                .WithMany(d => d.Licenses)
                .OnDelete(DeleteBehavior.NoAction);

            //device group
            modelBuilder.Entity<DeviceGroup>()
                .HasIndex(c => c.Name)
                .IsUnique();

            //playlist
            modelBuilder.Entity<Playlist>()
                .HasIndex(c => c.Name)
                .IsUnique();

            //sub playlist
            modelBuilder.Entity<SubPlaylist>()
                .HasOne(pi => pi.Playlist)
                .WithMany(p => p.SubPlaylists)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_playlist_id");

            //playlist item
            modelBuilder.Entity<PlaylistItem>()
                .HasOne(pi => pi.SubPlaylist)
                .WithMany(p => p.PlaylistItems)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_sub_playlist_id");

            //playlist item
            modelBuilder.Entity<User>()
                .HasOne(pi => pi.Organization)
                .WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_user_org_id");

            //playlist item
            modelBuilder.Entity<DeviceGroup>()
                .HasOne(pi => pi.Organization)
                .WithMany(p => p.DeviceGroups)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_group_org_id");

            //playlist and group many to many relationship
            modelBuilder.Entity<PlaylistGroup>()
                .HasOne(p => p.Playlist)
                .WithMany(p => p.PlaylistGroups)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlaylistGroup>()
                .HasOne(p => p.DeviceGroup)
                .WithMany(p => p.PlaylistGroups)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
