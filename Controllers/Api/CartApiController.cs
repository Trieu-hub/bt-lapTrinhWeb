using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models;
using untitled1.Models.DTOs;
using untitled1.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace untitled1.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Produces("application/json")]
    public class CartApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieve all items currently in the shopping cart along with totals.
        /// </summary>
        /// <returns>Full cart details with calculations.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CartResponseDto))]
        public IActionResult GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var response = MapToCartResponseDto(cart);
            return Ok(response);
        }

        /// <summary>
        /// Add a movie to the shopping cart.
        /// </summary>
        /// <param name="request">The movie ID and quantity to add.</param>
        /// <returns>Result status and updated cart count.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null || request.Quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ!" });
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == request.MovieId);
            if (movie == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm phim không tồn tại!" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.MovieId == request.MovieId);

            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    MovieId = movie.Id,
                    MovieTitle = movie.Title,
                    ImageUrl = movie.ImageUrl,
                    Price = movie.Price,
                    Quantity = request.Quantity
                });
            }
            else
            {
                cartItem.Quantity += request.Quantity;
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return Ok(new
            {
                success = true,
                message = $"Đã thêm '{movie.Title}' vào giỏ hàng thành công!",
                cartCount = cart.Sum(c => c.Quantity),
                cart = MapToCartResponseDto(cart)
            });
        }

        /// <summary>
        /// Update the quantity of a specific item in the cart.
        /// </summary>
        /// <param name="movieId">The ID of the movie to update.</param>
        /// <param name="request">The new quantity.</param>
        /// <returns>Result status and updated totals.</returns>
        [HttpPut("{movieId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateQuantity(int movieId, [FromBody] UpdateCartRequest request)
        {
            if (request == null || request.Quantity < 0)
            {
                return BadRequest(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ!" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.MovieId == movieId);

            if (item == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm không có trong giỏ hàng!" });
            }

            if (request.Quantity == 0)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity = request.Quantity;
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            var updatedCart = MapToCartResponseDto(cart);

            return Ok(new
            {
                success = true,
                message = "Cập nhật số lượng thành công!",
                cartCount = updatedCart.CartCount,
                itemTotal = item.TotalPrice.ToString("N0") + " VNĐ",
                subtotal = updatedCart.SubtotalFormatted,
                tax = updatedCart.TaxFormatted,
                total = updatedCart.TotalFormatted,
                cart = updatedCart
            });
        }

        /// <summary>
        /// Remove a specific item from the cart.
        /// </summary>
        /// <param name="movieId">The ID of the movie to remove.</param>
        /// <returns>Result status and updated totals.</returns>
        [HttpDelete("{movieId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveFromCart(int movieId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.MovieId == movieId);

            if (item == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm không có trong giỏ hàng!" });
            }

            cart.Remove(item);
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            var updatedCart = MapToCartResponseDto(cart);

            return Ok(new
            {
                success = true,
                message = "Đã xóa sản phẩm khỏi giỏ hàng!",
                cartCount = updatedCart.CartCount,
                subtotal = updatedCart.SubtotalFormatted,
                tax = updatedCart.TaxFormatted,
                total = updatedCart.TotalFormatted,
                cart = updatedCart
            });
        }

        /// <summary>
        /// Clear all items from the shopping cart.
        /// </summary>
        /// <returns>Result status.</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return Ok(new
            {
                success = true,
                message = "Giỏ hàng đã được xóa sạch!",
                cartCount = 0,
                cart = new CartResponseDto()
            });
        }

        // Helper mapper method
        private CartResponseDto MapToCartResponseDto(List<CartItem> cart)
        {
            var items = cart.Select(c => new CartItemDto
            {
                MovieId = c.MovieId,
                MovieTitle = c.MovieTitle,
                ImageUrl = c.ImageUrl,
                Price = c.Price,
                Quantity = c.Quantity,
                TotalPrice = c.TotalPrice
            }).ToList();

            decimal subtotal = items.Sum(i => i.TotalPrice);
            decimal tax = subtotal * 0.1m;
            decimal total = subtotal + tax;

            return new CartResponseDto
            {
                Items = items,
                CartCount = items.Sum(i => i.Quantity),
                Subtotal = subtotal,
                Tax = tax,
                Total = total
            };
        }
    }
}
