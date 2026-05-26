using HappyHome.Core.Enums;
using HappyHome.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HappyHome.Infrastructure;

public class HappyHomeDbContext : DbContext
{
    public HappyHomeDbContext(DbContextOptions<HappyHomeDbContext> options) : base(options) { }

    public DbSet<Klient> Klienter => Set<Klient>();
    public DbSet<Konsultation> Konsultationer => Set<Konsultation>();
    public DbSet<Terapeut> Terapeuter => Set<Terapeut>();
    public DbSet<Tidslucka> Tidsluckor => Set<Tidslucka>();
    public DbSet<Bokning> Bokningar => Set<Bokning>();
    public DbSet<Användare> Användare => Set<Användare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Klient>(e =>
        {
            e.Property(k => k.Förnamn).IsRequired().HasMaxLength(100);
            e.Property(k => k.Efternamn).IsRequired().HasMaxLength(100);
            e.Property(k => k.Personnummer).IsRequired().HasMaxLength(13);
            e.HasIndex(k => k.Personnummer).IsUnique();
            e.Property(k => k.Epost).IsRequired().HasMaxLength(200);
            e.Property(k => k.Telefon).HasMaxLength(40);
            e.HasIndex(k => k.Epost).IsUnique();
        });

        modelBuilder.Entity<Terapeut>(e =>
        {
            e.Property(t => t.Förnamn).IsRequired().HasMaxLength(100);
            e.Property(t => t.Efternamn).IsRequired().HasMaxLength(100);
            e.Property(t => t.Epost).IsRequired().HasMaxLength(200);
            e.HasIndex(t => t.Epost).IsUnique();

            var specConverter = new ValueConverter<List<TerapiTyp>, string>(
                v => string.Join(',', v.Select(x => x.ToString())),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<TerapiTyp>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => Enum.Parse<TerapiTyp>(s))
                       .ToList());

            var specComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<TerapiTyp>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())),
                v => v.ToList());

            e.Property(t => t.Specialiseringar)
                .HasConversion(specConverter)
                .Metadata.SetValueComparer(specComparer);
        });

        modelBuilder.Entity<Tidslucka>(e =>
        {
            e.HasOne(tl => tl.Terapeut)
                .WithMany(t => t.Tidsluckor)
                .HasForeignKey(tl => tl.TerapeutId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(tl => new { tl.TerapeutId, tl.Start });
        });

        modelBuilder.Entity<Bokning>(e =>
        {
            e.Property(b => b.AnledningTillBesok).HasMaxLength(2000);

            e.HasOne(b => b.Klient)
                .WithMany(k => k.Bokningar)
                .HasForeignKey(b => b.KlientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Tidslucka)
                .WithOne(tl => tl.Bokning!)
                .HasForeignKey<Bokning>(b => b.TidsluckaId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(b => b.TidsluckaId).IsUnique();
        });

        modelBuilder.Entity<Användare>(e =>
        {
            e.Property(a => a.Epost).IsRequired().HasMaxLength(200);
            e.HasIndex(a => a.Epost).IsUnique();
            e.Property(a => a.LösenordHash).IsRequired().HasMaxLength(200);

            e.HasOne(a => a.Klient)
                .WithMany()
                .HasForeignKey(a => a.KlientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Terapeut)
                .WithMany()
                .HasForeignKey(a => a.TerapeutId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Konsultation>(e =>
        {
            e.Property(k => k.Symptom).IsRequired().HasMaxLength(2000);
            e.Property(k => k.Bakgrund).IsRequired().HasMaxLength(4000);
            e.Property(k => k.Anteckningar).HasMaxLength(4000);
            e.Property(k => k.Diagnosförslag).HasMaxLength(1000);

            e.HasOne(k => k.Klient)
                .WithMany(c => c.Konsultationer)
                .HasForeignKey(k => k.KlientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(k => k.Terapeut)
                .WithMany(t => t.Konsultationer)
                .HasForeignKey(k => k.TerapeutId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(k => k.Bokning)
                .WithOne(b => b.Konsultation!)
                .HasForeignKey<Konsultation>(k => k.BokningId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(k => k.BokningId).IsUnique();
        });
    }
}
