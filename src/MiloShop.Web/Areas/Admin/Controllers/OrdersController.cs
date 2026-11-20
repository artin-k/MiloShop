using MiloShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiloShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class OrdersController : Controller
    {
        private readonly MiloDbContext _context;

        public OrdersController(MiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "سفارش‌ها";

            var orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            ViewBag.OrderUsers = await _context.Users
                .Where(user => orders.Select(order => order.UserId).Contains(user.Id))
                .AsNoTracking()
                .ToDictionaryAsync(user => user.Id);

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewBag.OrderUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == order.UserId);

            ViewData["Title"] = $"جزئیات سفارش #{order.Id}";
            return View(order);
        }
    }
}
