using RequestsManagement.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RequestsManagement.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Request> Requests => Set<Request>();

        public DbSet<RequestStatusChange> RequestStatusChanges => Set<RequestStatusChange>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        }

        private class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
        {
            public UtcDateTimeConverter()
                : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            {
            }
        }

        // מגדיר את הטבלאות: אורכי שדות, RowVersion, מפתח זר ואינדקסים
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Requests");

                entity.Property(r => r.Title).HasMaxLength(200).IsRequired();
                entity.Property(r => r.OrganizationName).HasMaxLength(200).IsRequired();
                entity.Property(r => r.AssignedTo).HasMaxLength(100);

                entity.Property(r => r.RowVersion).IsRowVersion();

                entity.HasIndex(r => r.CreatedAt);
                entity.HasIndex(r => new { r.Status, r.CreatedAt });
                entity.HasIndex(r => new { r.OrganizationName, r.CreatedAt })
                    .IncludeProperties(r => new { r.Status, r.Priority });
                entity.HasIndex(r => new { r.AssignedTo, r.CreatedAt })
                    .IncludeProperties(r => new { r.Status, r.Priority });
            });

            modelBuilder.Entity<RequestStatusChange>(entity =>
            {
                entity.ToTable("RequestStatusChanges");

                entity.Property(c => c.ChangedBy).HasMaxLength(100).IsRequired();

                entity.HasOne<Request>().WithMany().HasForeignKey(c => c.RequestId).OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.RequestId, c.ChangedAt });
            });
        }
    }
}
