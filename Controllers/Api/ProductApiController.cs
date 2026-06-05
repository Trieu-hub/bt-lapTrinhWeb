using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.DTOs;
using untitled1.Models.Entities;

namespace untitled1.Controllers
{
    /// <summary>
    /// Admin REST API for managing movies/products.
    /// Requires an active Admin session — log in at /Account/Auth before testing in Swagger.
    /// </summary>
    [ApiController]
    [Route("api/admin/products")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class ProductApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a paginated list of products with optional title search.
        /// </summary>
        /// <param name="search">Optional title keyword filter.</param>
        /// <param name="page">1-based page number (default: 1).</param>
        /// <param name="pageSize">Items per page, capped at 50 (default: 10).</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(string? search, int page = 1, int pageSize = 10)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(1, page);

            var query = _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search));

            var total = await query.CountAsync();

            var movies = await query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = movies.Select(MapToDto),
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        /// <summary>
        /// Get a single product by ID.
        /// </summary>
        /// <param name="id">Product ID.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm với ID {id}." });

            return Ok(MapToDto(movie));
        }

        /// <summary>
        /// Create a new product.
        /// </summary>
        /// <param name="request">Product fields. CategoryIds links the movie to existing categories.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            var movie = new Movie
            {
                Title      = request.Title,
                ImageUrl   = request.ImageUrl,
                Year       = request.Year,
                Genre      = request.Genre,
                IsTVSeries = request.IsTVSeries,
                Price      = request.Price
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            if (request.CategoryIds.Length > 0)
            {
                foreach (var catId in request.CategoryIds)
                    _context.MovieCategories.Add(new MovieCategory { MovieId = movie.Id, CategoryId = catId });
                await _context.SaveChangesAsync();
            }

            await _context.Entry(movie).Collection(m => m.MovieCategories)
                .Query().Include(mc => mc.Category).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, MapToDto(movie));
        }

        /// <summary>
        /// Update an existing product. Replaces all category links with the supplied CategoryIds.
        /// </summary>
        /// <param name="id">Product ID to update.</param>
        /// <param name="request">Updated product fields.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm với ID {id}." });

            movie.Title      = request.Title;
            movie.ImageUrl   = request.ImageUrl;
            movie.Year       = request.Year;
            movie.Genre      = request.Genre;
            movie.IsTVSeries = request.IsTVSeries;
            movie.Price      = request.Price;

            var existing = _context.MovieCategories.Where(mc => mc.MovieId == id);
            _context.MovieCategories.RemoveRange(existing);

            foreach (var catId in request.CategoryIds)
                _context.MovieCategories.Add(new MovieCategory { MovieId = id, CategoryId = catId });

            await _context.SaveChangesAsync();

            await _context.Entry(movie).Collection(m => m.MovieCategories)
                .Query().Include(mc => mc.Category).LoadAsync();

            return Ok(MapToDto(movie));
        }

        /// <summary>
        /// Delete a product by ID. Cascades to MovieCategories and Episodes.
        /// </summary>
        /// <param name="id">Product ID to delete.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm với ID {id}." });

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã xóa sản phẩm \"{movie.Title}\" thành công." });
        }

        private static ProductDto MapToDto(Movie movie) => new()
        {
            Id         = movie.Id,
            Title      = movie.Title,
            ImageUrl   = movie.ImageUrl,
            Year       = movie.Year,
            Genre      = movie.Genre,
            IsTVSeries = movie.IsTVSeries,
            Price      = movie.Price,
            Categories = movie.MovieCategories.Select(mc => mc.Category.Name).ToList()
        };
    }
}
