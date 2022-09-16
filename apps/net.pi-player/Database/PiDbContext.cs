using System;
using System.Diagnostics;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace adworks.pi_player
{
    public class PiDbContext: DbContext
    {
        public PiDbContext()
        {
        }
        
        public PiDbContext(DbContextOptions<PiDbContext> options)
            : base(options)
        {
        }
        

        public virtual DbSet<Record> Records { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<Device> Devices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = AppConfig.GetConfig();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            Debug.WriteLine("PiDbContext uses the following connection string to connect to maria db server :: " + connectionString);
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 23)));
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        
            //organizations
            modelBuilder.Entity<Record>().ToTable("Records");
            modelBuilder.Entity<Record>()
                .HasKey(b => b.Id);
            
            
            modelBuilder.Entity<Playlist>().ToTable("Playlists");
            
            modelBuilder.Entity<Device>().ToTable("Devices");
        }    
        
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}