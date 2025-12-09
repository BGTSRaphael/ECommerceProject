using Microsoft.AspNetCore.Mvc;
using ECommerceProject.Models;
using Microsoft.AspNetCore.Http;

namespace ECommerceProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Trang Dashboard (Cần đăng nhập)
        public IActionResult Index()
        {
            // Kiểm tra Session, nếu chưa đăng nhập Admin thì đá về trang Login
            if (HttpContext.Session.GetString("AdminRole") != "Admin")
                return RedirectToAction("Login");

            // Thống kê cơ bản
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalOrders = _context.Carts.Count();
            // Tính tổng doanh thu (nếu CartDetails rỗng thì trả về 0)
            ViewBag.TotalRevenue = _context.CartDetails.Any()
                ? _context.CartDetails.Sum(d => d.Quantity * d.PriceAtTime)
                : 0;

            return View();
        }

        // 2. Trang Đăng nhập Admin (GET)
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì vào thẳng Dashboard
            if (HttpContext.Session.GetString("AdminRole") == "Admin")
                return RedirectToAction("Index");

            return View();
        }

        // 3. Xử lý Đăng nhập (POST) - SỬA ĐOẠN NÀY
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Tìm user trong DB có:
            // - Email trùng username
            // - Mật khẩu trùng password
            // - Role bắt buộc phải là "Admin"
            var user = _context.Customers
                .FirstOrDefault(c => c.Email == username
                                  && c.PasswordHash == password
                                  && c.Role == "Admin");

            if (user != null)
            {
                // Đăng nhập thành công -> Lưu quyền Admin vào Session
                HttpContext.Session.SetString("AdminRole", "Admin");
                HttpContext.Session.SetString("AdminName", user.LastName + " " + user.FirstName);

                return RedirectToAction("Index");
            }

            // Đăng nhập thất bại
            ViewBag.Error = "Tài khoản không tồn tại hoặc không có quyền Admin!";
            return View();
        }

        // 4. Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminRole");
            HttpContext.Session.Remove("AdminName");
            return RedirectToAction("Login");
        }
    }
}