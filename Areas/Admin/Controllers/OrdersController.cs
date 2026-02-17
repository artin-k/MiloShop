using avatCo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace avatCo.Areas.Admin.Controllers
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
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewData["Title"] = $"جزئیات سفارش #{order.Id}";
            return View(order);
        }
    }
}
