using Microsoft.EntityFrameworkCore;

namespace application {    
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Experiment> Experiments { get; set; }
        public DbSet<Rect> Rects { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<IndividualSquare> IndividualSquares { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost; port=3306; database=genetic_algorithm; user=root; password=;", 
                new MySqlServerVersion(new Version(10, 4, 32))
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Experiment>()
                .HasKey(exp => exp.Id);
            
            modelBuilder.Entity<Rect>()
                .HasKey(rect => rect.Id);

            modelBuilder.Entity<Rect>()
                .HasOne(rect => rect.Experiment)
                .WithMany(exp => exp.Rects);

            modelBuilder.Entity<Individual>()
                .HasKey(ind => ind.Id);

            modelBuilder.Entity<Individual>()
                .HasOne(ind => ind.Experiment)
                .WithMany(exp => exp.Individuals);

            modelBuilder.Entity<IndividualSquare>()
                .HasKey(indsq => indsq.Id);

            modelBuilder.Entity<IndividualSquare>()
                .HasOne(indsq => indsq.Individual)
                .WithMany(ind => ind.IndividualSquares);
        }
    }
}