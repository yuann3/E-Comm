using Microsoft.EntityFrameworkCore;

namespace E_Comm.Models
{
    public class EntertainmentGuildContext : DbContext
    {
        public EntertainmentGuildContext(DbContextOptions<EntertainmentGuildContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<GameGenre> GameGenres { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Patron> Patrons { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<Stocktake> Stocktakes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ProductsInOrder> ProductsInOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity mappings to match existing database schema
            modelBuilder.Entity<Genre>(entity =>
            {
                entity.ToTable("Genre");
                entity.HasKey(e => e.GenreID);
                entity.Property(e => e.GenreID).HasColumnName("genreID");
            });

            // Configure sub-genre tables
            modelBuilder.Entity<BookGenre>(entity =>
            {
                entity.ToTable("Book_genre");
                entity.HasKey(e => e.SubGenreID);
                entity.Property(e => e.SubGenreID).HasColumnName("subGenreID").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<MovieGenre>(entity =>
            {
                entity.ToTable("Movie_genre");
                entity.HasKey(e => e.SubGenreID);
                entity.Property(e => e.SubGenreID).HasColumnName("subGenreID").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<GameGenre>(entity =>
            {
                entity.ToTable("Game_genre");
                entity.HasKey(e => e.SubGenreID);
                entity.Property(e => e.SubGenreID).HasColumnName("subGenreID").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.GenreID).HasColumnName("Genre");
                entity.HasOne(p => p.Genre)
                      .WithMany(g => g.Products)
                      .HasForeignKey(p => p.GenreID);
                entity.HasOne(p => p.LastUpdatedByUser)
                      .WithMany()
                      .HasForeignKey(p => p.LastUpdatedBy)
                      .HasPrincipalKey(u => u.UserName);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.UserName);
                entity.Property(e => e.IsAdmin).HasColumnName("isAdmin");
            });

            modelBuilder.Entity<Patron>(entity =>
            {
                entity.ToTable("Patrons");
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserID).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Source>(entity =>
            {
                entity.ToTable("Source");
                entity.HasKey(e => e.SourceId);
                entity.Property(e => e.SourceId).HasColumnName("sourceid").ValueGeneratedOnAdd();
                entity.Property(e => e.GenreID).HasColumnName("Genre");
                entity.HasOne(s => s.Genre)
                      .WithMany(g => g.Sources)
                      .HasForeignKey(s => s.GenreID);
            });

            modelBuilder.Entity<Stocktake>(entity =>
            {
                entity.ToTable("Stocktake");
                entity.HasKey(e => e.ItemId);
                entity.Property(e => e.ItemId).ValueGeneratedOnAdd();
                entity.HasOne(s => s.Source)
                      .WithMany(so => so.Stocktakes)
                      .HasForeignKey(s => s.SourceId);
                entity.HasOne(s => s.Product)
                      .WithMany(p => p.Stocktakes)
                      .HasForeignKey(s => s.ProductId);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.OrderID);
                entity.Property(e => e.OrderID).ValueGeneratedOnAdd();
                entity.Property(e => e.CustomerId).HasColumnName("customer");
                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("TO");
                entity.HasKey(e => e.CustomerID);
                entity.Property(e => e.CustomerID).HasColumnName("customerID").ValueGeneratedOnAdd();
                entity.HasOne(c => c.Patron)
                      .WithMany(p => p.Customers)
                      .HasForeignKey(c => c.PatronId);
            });

            modelBuilder.Entity<ProductsInOrder>(entity =>
            {
                entity.ToTable("ProductsInOrders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProduktId).HasColumnName("produktId");
                entity.HasOne(p => p.Order)
                      .WithMany(o => o.ProductsInOrders)
                      .HasForeignKey(p => p.OrderId);
                entity.HasOne(p => p.Stocktake)
                      .WithMany(s => s.ProductsInOrders)
                      .HasForeignKey(p => p.ProduktId);
            });
        }
    }
} 