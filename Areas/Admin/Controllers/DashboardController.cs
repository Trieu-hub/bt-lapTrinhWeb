using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalMovies    = await _context.Movies.CountAsync();
            ViewBag.TotalTVSeries  = await _context.Movies.CountAsync(m => m.IsTVSeries);
            ViewBag.TotalFilms     = await _context.Movies.CountAsync(m => !m.IsTVSeries);
            ViewBag.TotalUsers     = await _userManager.Users.CountAsync();
            ViewBag.RecentMovies   = await _context.Movies
                                         .OrderByDescending(m => m.Id)
                                         .Take(5)
                                         .ToListAsync();
            return View();
        }
    }
}
