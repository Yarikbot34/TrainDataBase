using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain.Classes;

namespace DB;

public class AppDbContext : DbContext
{
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Train> Trains => Set<Train>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<MapSchema> MapSchemas => Set<MapSchema>();
    public DbSet<MapCell> MapCells => Set<MapCell>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public AppDbContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Station
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Class).HasMaxLength(50);
            
            entity.HasOne(t => t.Transaction)
                .WithMany()
                .HasForeignKey(t => t.TransactionId);
        });

        //Route
        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RouteNumber).IsRequired().HasMaxLength(50);
            
            entity.HasMany(r => r.Trains)
                  .WithOne(t => t.Route)
                  .HasForeignKey(t => t.RouteId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.OwnsOne(r => r.Casual, b =>
            {
                b.Property(p => p.Count).HasColumnName("Casual_Count");
                b.Property(p => p.Payment).HasColumnName("Casual_Payment");
                b.Property(p => p.WayLength).HasColumnName("Casual_WayLength");
                b.Property(p => p.PaymentBySubject).HasColumnName("Casual_PaymentBySubject");
            });

            entity.OwnsOne(r => r.Student, b =>
            {
                b.Property(p => p.Count).HasColumnName("Student_Count");
                b.Property(p => p.Payment).HasColumnName("Student_Payment");
                b.Property(p => p.WayLength).HasColumnName("Student_WayLength");
                b.Property(p => p.PaymentBySubject).HasColumnName("Student_PaymentBySubject");
            });

            entity.OwnsOne(r => r.FedBenefit, b =>
            {
                b.Property(p => p.Count).HasColumnName("FedBenefit_Count");
                b.Property(p => p.Payment).HasColumnName("FedBenefit_Payment");
                b.Property(p => p.WayLength).HasColumnName("FedBenefit_WayLength");
                b.Property(p => p.PaymentBySubject).HasColumnName("FedBenefit_PaymentBySubject");
            });

            entity.OwnsOne(r => r.RegBenefit, b =>
            {
                b.Property(p => p.Count).HasColumnName("RegBenefit_Count");
                b.Property(p => p.Payment).HasColumnName("RegBenefit_Payment");
                b.Property(p => p.WayLength).HasColumnName("RegBenefit_WayLength");
                b.Property(p => p.PaymentBySubject).HasColumnName("RegBenefit_PaymentBySubject");
            });

            entity.OwnsOne(r => r.Another, b =>
            {
                b.Property(p => p.Count).HasColumnName("Another_Count");
                b.Property(p => p.Payment).HasColumnName("Another_Payment");
                b.Property(p => p.WayLength).HasColumnName("Another_WayLength");
                b.Property(p => p.PaymentBySubject).HasColumnName("Another_PaymentBySubject");
            });
            
            entity.HasOne(t => t.Transaction)
                .WithMany()
                .HasForeignKey(t => t.TransactionId);
        });

        //Train 
        modelBuilder.Entity<Train>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Number).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Period).HasMaxLength(50);
            
            entity.HasOne(t => t.StationFrom)
                  .WithMany()
                  .HasForeignKey(t => t.StationFromId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.StationMiddle)
                  .WithMany()
                  .HasForeignKey(t => t.StationMiddleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.StationTo)
                  .WithMany()
                  .HasForeignKey(t => t.StationToId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Transaction)
                .WithMany()
                .HasForeignKey(t => t.TransactionId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Year).IsRequired();
            entity.Property(t => t.Month).IsRequired();
            entity.Property(t => t.UnitsGet).IsRequired();
        });
        
        //Map Schema
        modelBuilder.Entity<MapSchema>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasMany(s => s.MapCells)
                .WithOne(c => c.Schema)
                .HasForeignKey(c => c.SchemaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MapCell>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Schema)
                    .WithMany(s => s.MapCells)
                    .HasForeignKey(c => c.SchemaId);

                entity.OwnsOne(c => c.Data, cd =>
                {
                    cd.Property(cd => cd.Label).HasColumnName("Label");
                    cd.Property(cd => cd.Load).HasColumnName("Load");
                });

                entity.OwnsOne(c => c.Position, cp =>
                {
                    cp.Property(cp => cp.X).HasColumnName("x");
                    cp.Property(cp => cp.Y).HasColumnName("y");
                });

                entity.OwnsOne(c => c.Source, cs =>
                {
                    cs.Property(cs => cs.Cell).HasColumnName("Source");
                });

                entity.OwnsOne(c => c.Target, ct =>
                {
                    ct.Property(ct => ct.Cell).HasColumnName("Target");
                });

            }
        );
    }
}