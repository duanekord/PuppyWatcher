using Microsoft.EntityFrameworkCore;
using PuppyWeightWatcher.Shared.Models;

namespace PuppyWeightWatcher.ApiService.Data;

public class PuppyDbContext(DbContextOptions<PuppyDbContext> options) : DbContext(options)
{
    public DbSet<Puppy> Puppies => Set<Puppy>();
    public DbSet<Litter> Litters => Set<Litter>();
    public DbSet<LitterMember> LitterMembers => Set<LitterMember>();
    public DbSet<WeightEntry> WeightEntries => Set<WeightEntry>();
    public DbSet<ShotRecord> ShotRecords => Set<ShotRecord>();
    public DbSet<PuppyPhoto> PuppyPhotos => Set<PuppyPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Puppy>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.CollarColor).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Name).HasMaxLength(100);
            entity.Property(p => p.Breed).HasMaxLength(100);
            entity.Property(p => p.Gender).HasMaxLength(20);

            entity.HasMany<ShotRecord>()
                .WithOne()
                .HasForeignKey(s => s.PuppyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<WeightEntry>()
                .WithOne()
                .HasForeignKey(w => w.PuppyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<PuppyPhoto>()
                .WithOne()
                .HasForeignKey(ph => ph.PuppyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(p => p.ShotRecords);
            entity.Property(p => p.OwnerId).HasMaxLength(256);
            entity.HasIndex(p => p.LitterId);
            entity.HasIndex(p => p.OwnerId);
        });

        modelBuilder.Entity<Litter>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired().HasMaxLength(100);
            entity.Property(l => l.Breed).HasMaxLength(100);
            entity.Property(l => l.Notes).HasMaxLength(500);

            entity.HasMany<Puppy>()
                .WithOne()
                .HasForeignKey(p => p.LitterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Ignore(l => l.PuppyCount);
            entity.Ignore(l => l.UserRole);
        });

        modelBuilder.Entity<LitterMember>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.UserId).IsRequired().HasMaxLength(256);
            entity.HasIndex(m => new { m.LitterId, m.UserId }).IsUnique();
            entity.HasIndex(m => m.UserId);

            entity.HasOne<Litter>()
                .WithMany()
                .HasForeignKey(m => m.LitterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeightEntry>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Notes).HasMaxLength(500);
            entity.HasIndex(w => w.PuppyId);
        });

        modelBuilder.Entity<ShotRecord>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.VaccinationType).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Notes).HasMaxLength(500);
            entity.Property(s => s.AdministeredBy).HasMaxLength(100);
            entity.HasIndex(s => s.PuppyId);
        });

        modelBuilder.Entity<PuppyPhoto>(entity =>
        {
            entity.HasKey(ph => ph.Id);
            entity.Property(ph => ph.FileName).IsRequired().HasMaxLength(255);
            entity.Property(ph => ph.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(ph => ph.Caption).HasMaxLength(500);
            entity.HasIndex(ph => new { ph.PuppyId, ph.IsProfilePhoto });
        });
    }
}
