using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models;
using untitled1.Models.Entities;
using untitled1.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace untitled1.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display Cart Page
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            
            decimal subtotal = cart.Sum(c => c.TotalPrice);
            decimal tax = subtotal * 0.1m; // 10% VAT
            decimal total = subtotal + tax;

            ViewBag.Subtotal = subtotal;
            ViewBag.Tax = tax;
            ViewBag.Total = total;

            return View(cart);
        }

        // Add Item to Cart (AJAX API)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity)
        {
            if (quantity <= 0) quantity = 1;

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.MovieId == id);

            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    MovieId = movie.Id,
                    MovieTitle = movie.Title,
                    ImageUrl = movie.ImageUrl,
                    Price = movie.Price,
                    Quantity = quantity
                });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new { 
                success = true, 
                message = $"Đã thêm '{movie.Title}' vào giỏ hàng thành công!", 
                cartCount = cart.Sum(c => c.Quantity) 
            });
        }

        // Update Quantity (AJAX API)
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.MovieId == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            decimal subtotal = cart.Sum(c => c.TotalPrice);
            decimal tax = subtotal * 0.1m;
            decimal total = subtotal + tax;

            return Json(new { 
                success = true, 
                cartCount = cart.Sum(c => c.Quantity), 
                itemTotal = item != null ? item.TotalPrice.ToString("N0") + " VNĐ" : "0 VNĐ",
                subtotal = subtotal.ToString("N0") + " VNĐ",
                tax = tax.ToString("N0") + " VNĐ",
                total = total.ToString("N0") + " VNĐ"
            });
        }

        // Remove from Cart (AJAX API)
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.MovieId == id);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            decimal subtotal = cart.Sum(c => c.TotalPrice);
            decimal tax = subtotal * 0.1m;
            decimal total = subtotal + tax;

            return Json(new { 
                success = true, 
                cartCount = cart.Sum(c => c.Quantity), 
                subtotal = subtotal.ToString("N0") + " VNĐ",
                tax = tax.ToString("N0") + " VNĐ",
                total = total.ToString("N0") + " VNĐ"
            });
        }

        // Get Cart Count (AJAX API)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return Json(new { count = cart.Sum(c => c.Quantity) });
        }
    }
}
