using Microsoft.EntityFrameworkCore;
using Backend_QLTE.ChildService.Domain.Entities;

namespace Backend_QLTE.ChildService.Infrastructure.Data
{
    public class ChildDbContext : DbContext
    {
        public ChildDbContext(DbContextOptions<ChildDbContext> options) : base(options) { }

        public DbSet<Child> Children { get; set; }
        public DbSet<LocationHistory> LocationHistories { get; set; }
        public DbSet<SafeZone> SafeZones { get; set; }
        public DbSet<DangerZone> DangerZones { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<SOSRequest> SOSRequests { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // quan hệ 1-1: Child - DeviceInfo
            modelBuilder.Entity<Child>()
                .HasOne(c => c.DeviceInfo)
                .WithOne(d => d.Child)
                .HasForeignKey<DeviceInfo>(d => d.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            // quan hệ 1-n: Child - LocationHistory
            modelBuilder.Entity<Child>()
                .HasMany(c => c.LocationHistories)
                .WithOne(l => l.Child)
                .HasForeignKey(l => l.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
