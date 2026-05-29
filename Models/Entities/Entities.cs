namespace untitled1.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation property for many-to-many
        public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty; // Keeps textual genre overview for reference
        public bool IsTVSeries { get; set; }
        public decimal Price { get; set; } = 99000;

        // Many-to-many relationship with Categories via join table
        public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();

        // One-to-many relationship with Episodes
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();

        // One-to-many relationship with MovieImages
        public ICollection<MovieImage> MovieImages { get; set; } = new List<MovieImage>();
    }

    // Join Entity for Many-to-Many Movie <-> Category
    public class MovieCategory
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }

    // Episode Entity for One-to-Many Movie <-> Episode
    public class Episode
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string VideoUrl { get; set; } = string.Empty;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }

    // MovieImage Entity for One-to-Many Movie <-> MovieImage
    public class MovieImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        
        public string Status { get; set; } = "Completed";

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
