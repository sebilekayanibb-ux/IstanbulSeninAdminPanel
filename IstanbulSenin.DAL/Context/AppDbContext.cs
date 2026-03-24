using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq;

namespace IstanbulSenin.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Section> Sections { get; set; }
        public DbSet<MiniAppItem> MiniAppItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tablolarını oluşturur (önce çağrılmalı)
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Section>()
                .HasMany(s => s.Items)
                .WithMany(m => m.Sections);

            // Notification → NotificationLog one-to-many ilişkisi
            modelBuilder.Entity<Notification>()
                .HasMany(n => n.Logs)
                .WithOne(l => l.Notification)
                .HasForeignKey(l => l.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Permissions listesini JSON olarak tek sütunda saklar
            modelBuilder.Entity<MiniAppItem>()
                .Property(e => e.Permissions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v),
                    v => string.IsNullOrWhiteSpace(v) ? new List<PermissionType>() : JsonSerializer.Deserialize<List<PermissionType>>(v) ?? new List<PermissionType>())
                .Metadata
                .SetValueComparer(new ValueComparer<List<PermissionType>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Plugins listesini JSON olarak tek sütunda saklar
            modelBuilder.Entity<MiniAppItem>()
                .Property(e => e.Plugins)
                .HasConversion(
                    v => JsonSerializer.Serialize(v),
                    v => string.IsNullOrWhiteSpace(v) ? new List<PluginType>() : JsonSerializer.Deserialize<List<PluginType>>(v) ?? new List<PluginType>())
                .Metadata
                .SetValueComparer(new ValueComparer<List<PluginType>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // QRCode → AppUser foreign key ilişkisi
            modelBuilder.Entity<QRCode>()
                .HasOne(q => q.User)
                .WithMany()
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // QRCode indeksleri
            modelBuilder.Entity<QRCode>()
                .HasIndex(q => q.Code)
                .IsUnique();

            modelBuilder.Entity<QRCode>()
                .HasIndex(q => q.UserId);

            modelBuilder.Entity<QRCode>()
                .HasIndex(q => q.ExpiresAt);
        }
    }
}
