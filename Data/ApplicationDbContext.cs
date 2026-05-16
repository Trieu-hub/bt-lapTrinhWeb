using Microsoft.EntityFrameworkCore;
using untitled1.Models.Entities;

namespace untitled1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Hành Động" },
                new Category { Id = 2, Name = "Kinh Dị" },
                new Category { Id = 3, Name = "Viễn Tưởng" },
                new Category { Id = 4, Name = "Tình Cảm" }
            );

            modelBuilder.Entity<Movie>().HasData(
                new Movie { Id = 1, Title = "Avengers: Endgame", CategoryId = 1, ImageUrl = "/images/movies/1.jpg", Year = 2019, Genre = "Action" },
                new Movie { Id = 2, Title = "John Wick 4", CategoryId = 1, ImageUrl = "/images/movies/2.jpg", Year = 2023, Genre = "Action" },
                new Movie { Id = 3, Title = "The Conjuring", CategoryId = 2, ImageUrl = "/images/movies/8.jpg", Year = 2013, Genre = "Horror" },
                new Movie { Id = 4, Title = "Interstellar", CategoryId = 3, ImageUrl = "/images/movies/7.jpg", Year = 2014, Genre = "Sci-Fi" },
                new Movie { Id = 5, Title = "Inception", CategoryId = 3, ImageUrl = "/images/movies/6.jpg", Year = 2010, Genre = "Sci-Fi" },
                new Movie { Id = 6, Title = "La La Land", CategoryId = 4, ImageUrl = "/images/movies/5.jpg", Year = 2016, Genre = "Romance" },
                new Movie { Id = 7, Title = "Breaking Bad", CategoryId = 1, ImageUrl = "/images/movies/4.jpg", Year = 2008, Genre = "Drama/Action" },
                new Movie { Id = 8, Title = "Wednesday", CategoryId = 2, ImageUrl = "/images/movies/3.jpg", Year = 2022, Genre = "Horror/Fantasy" }
            );
        }
    }
}
