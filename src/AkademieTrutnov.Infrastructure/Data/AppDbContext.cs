using AkademieTrutnov.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkademieTrutnov.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Kurz> Kurzy => Set<Kurz>();
    public DbSet<Ucebna> Ucebny => Set<Ucebna>();
    public DbSet<Termin> Terminy => Set<Termin>();
    public DbSet<Lektor> Lektori => Set<Lektor>();
    public DbSet<TerminLektor> TerminLektori => Set<TerminLektor>();
    public DbSet<Student> Studenti => Set<Student>();
    public DbSet<Prihlaska> Prihlasky => Set<Prihlaska>();
    public DbSet<DenVyuky> DnyVyuky => Set<DenVyuky>();
    public DbSet<Dochazka> Dochazky => Set<Dochazka>();
    public DbSet<Osvedceni> Osvedceni => Set<Osvedceni>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kurz
        modelBuilder.Entity<Kurz>(b =>
        {
            b.HasKey(k => k.Id);
            b.Property(k => k.Nazev).IsRequired().HasMaxLength(200);
            b.Property(k => k.Cena).HasPrecision(12, 2);
        });

        // Ucebna
        modelBuilder.Entity<Ucebna>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Nazev).IsRequired().HasMaxLength(100);
            b.Property(u => u.Adresa).HasMaxLength(250);
        });

        // Termin
        modelBuilder.Entity<Termin>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasOne(t => t.Kurz)
                .WithMany(k => k.Terminy)
                .HasForeignKey(t => t.KurzId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.Ucebna)
                .WithMany(u => u.Terminy)
                .HasForeignKey(t => t.UcebnaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TerminLektor (Vazba M:N s dodatečnými atributy)
        modelBuilder.Entity<TerminLektor>(b =>
        {
            b.HasKey(tl => new { tl.TerminId, tl.LektorId });

            b.HasOne(tl => tl.Termin)
                .WithMany(t => t.TerminLektori)
                .HasForeignKey(tl => tl.TerminId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(tl => tl.Lektor)
                .WithMany(l => l.TerminLektori)
                .HasForeignKey(tl => tl.LektorId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(tl => tl.Role).HasMaxLength(100);
        });

        // Student
        modelBuilder.Entity<Student>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Jmeno).IsRequired().HasMaxLength(100);
            b.Property(s => s.Prijmeni).IsRequired().HasMaxLength(100);
            b.Property(s => s.Email).IsRequired().HasMaxLength(150);
            b.Property(s => s.Telefon).HasMaxLength(50);
        });

        // Prihlaska
        modelBuilder.Entity<Prihlaska>(b =>
        {
            b.HasKey(p => p.Id);
            
            b.HasOne(p => p.Student)
                .WithMany(s => s.Prihlasky)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.Termin)
                .WithMany(t => t.Prihlasky)
                .HasForeignKey(p => p.TerminId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Property(p => p.DuvodZruseni).HasMaxLength(500);
            b.Property(p => p.PoznamkaAdmina).HasMaxLength(500);
        });

        // DenVyuky
        modelBuilder.Entity<DenVyuky>(b =>
        {
            b.HasKey(d => d.Id);

            b.HasOne(d => d.Termin)
                .WithMany(t => t.DnyVyuky)
                .HasForeignKey(d => d.TerminId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(d => d.Lektor)
                .WithMany()
                .HasForeignKey(d => d.LektorId)
                .OnDelete(DeleteBehavior.SetNull);

            b.Property(d => d.Tema).HasMaxLength(250);
        });

        // Dochazka
        modelBuilder.Entity<Dochazka>(b =>
        {
            b.HasKey(doc => doc.Id);

            b.HasOne(doc => doc.DenVyuky)
                .WithMany(d => d.Dochazky)
                .HasForeignKey(doc => doc.DenVyukyId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(doc => doc.Student)
                .WithMany(s => s.Dochazky)
                .HasForeignKey(doc => doc.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Property(doc => doc.Poznamka).HasMaxLength(250);
        });

        // Osvedceni (Striktně unikátní evidenční číslo dle zadání)
        modelBuilder.Entity<Osvedceni>(b =>
        {
            b.HasKey(o => o.Id);

            b.HasOne(o => o.Prihlaska)
                .WithOne(p => p.Osvedceni)
                .HasForeignKey<Osvedceni>(o => o.PrihlaskaId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(o => o.CisloOsvedceni).IsUnique();
            b.Property(o => o.CisloOsvedceni).IsRequired().HasMaxLength(50);
            b.Property(o => o.Vystavil).IsRequired().HasMaxLength(150);
        });
    }
}
