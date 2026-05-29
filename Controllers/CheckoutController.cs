using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;
using untitled1.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace untitled1.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Checkout
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Pre-fill ViewModel
            var model = new CheckoutViewModel
            {
                CustomerName = user.FullName,
                CustomerEmail = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            decimal subtotal = cart.Sum(c => c.TotalPrice);
            decimal tax = subtotal * 0.1m;
            decimal total = subtotal + tax;

            ViewBag.Cart = cart;
            ViewBag.Subtotal = subtotal;
            ViewBag.Tax = tax;
            ViewBag.Total = total;

            return View(model);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                decimal s = cart.Sum(c => c.TotalPrice);
                ViewBag.Cart = cart;
                ViewBag.Subtotal = s;
                ViewBag.Tax = s * 0.1m;
                ViewBag.Total = s + (s * 0.1m);
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            decimal subtotal = cart.Sum(c => c.TotalPrice);
            decimal tax = subtotal * 0.1m;
            decimal total = subtotal + tax;

            var order = new Order
            {
                UserId = user.Id,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                PhoneNumber = model.PhoneNumber,
                Subtotal = subtotal,
                Tax = tax,
                Total = total,
                OrderDate = DateTime.UtcNow,
                Status = "Completed",
                OrderItems = cart.Select(item => new OrderItem
                {
                    MovieId = item.MovieId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear session cart
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Success", new { id = order.Id });
        }

        // GET: /Checkout/Success/{id}
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Movie)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
