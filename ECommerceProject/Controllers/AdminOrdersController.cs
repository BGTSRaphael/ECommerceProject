using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;

namespace ECommerceProject.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Middleware kiểm tra Admin
        private bool IsAdmin() => HttpContext.Session.GetString("AdminRole") == "Admin";

        // 1. Danh sách đơn hàng (CÓ BỘ LỌC)
        // status: 0=Chờ, 1=Đang giao, 2=Đã giao, 3=Hủy
        public async Task<IActionResult> Index(int status = 0) // Mặc định vào là xem Chờ xác nhận (0)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");

            var orders = await _context.Carts
                .Include(c => c.CartDetails)
                .Include(c => c.Customer)
                .Where(c => c.OrderStatus == status) // Lọc theo trạng thái
                .OrderByDescending(c => c.DateCreated)
                .ToListAsync();

            ViewBag.CurrentStatus = status; // Để View biết đang ở tab nào
            return View(orders);
        }

        // 2. Xem chi tiết
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");

            var order = await _context.Carts
               .Include(c => c.Customer)
               .Include(c => c.CartDetails)
               .ThenInclude(d => d.Product)
               .FirstOrDefaultAsync(x => x.CartId == id);

            return View(order);
        }

        // 3. Cập nhật trạng thái đơn hàng (POST)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int newStatus)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");

            var order = await _context.Carts.FindAsync(id);
            if (order != null)
            {
                order.OrderStatus = newStatus;
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            // Quay lại trang danh sách với trạng thái vừa cập nhật để admin thấy sự thay đổi
            return RedirectToAction("Index", new { status = newStatus });
        }
    }
}