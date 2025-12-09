using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;
using Microsoft.AspNetCore.Http;

namespace ECommerceProject.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách đơn hàng
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (customerIdStr == null) return RedirectToAction("Login", "Account");

            int customerId = int.Parse(customerIdStr);

            // Lấy danh sách đơn hàng của khách, sắp xếp mới nhất lên đầu
            var orders = await _context.Carts
                .Include(c => c.CartDetails) // Kèm theo chi tiết để tính tổng tiền
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.DateCreated)
                .ToListAsync();

            return View(orders);
        }

        // 2. Xem chi tiết 1 đơn hàng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (customerIdStr == null) return RedirectToAction("Login", "Account");
            int customerId = int.Parse(customerIdStr);

            var order = await _context.Carts
                .Include(c => c.CartDetails)
                .ThenInclude(d => d.Product) // Kèm thông tin sản phẩm (để lấy tên, ảnh)
                .FirstOrDefaultAsync(m => m.CartId == id && m.CustomerId == customerId);

            if (order == null) return NotFound();

            return View(order);
        }

        // --- 3. CHỨC NĂNG HỦY ĐƠN HÀNG (MỚI THÊM) ---
        public async Task<IActionResult> CancelOrder(int id)
        {
            // A. Kiểm tra đăng nhập
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (customerIdStr == null) return RedirectToAction("Login", "Account");
            int customerId = int.Parse(customerIdStr);

            // B. Tìm đơn hàng (Phải đúng ID và đúng Chủ sở hữu)
            var order = await _context.Carts.FirstOrDefaultAsync(c => c.CartId == id && c.CustomerId == customerId);

            if (order != null)
            {
                // C. Kiểm tra trạng thái: Chỉ cho hủy nếu đang là 'Chờ xác nhận' (0)
                if (order.OrderStatus == 0)
                {
                    order.OrderStatus = 3; // Chuyển sang trạng thái 3 (Đã hủy)
                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Đã hủy đơn hàng thành công!";
                }
                else
                {
                    // Nếu Admin đã chuyển trạng thái sang Giao hàng (1) hoặc Đã giao (2)
                    TempData["Error"] = "Đơn hàng đang được xử lý hoặc đã giao, không thể hủy!";
                }
            }

            // Quay lại trang danh sách
            return RedirectToAction(nameof(Index));
        }
    }
}