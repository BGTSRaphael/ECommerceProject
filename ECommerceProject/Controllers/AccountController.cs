using Microsoft.AspNetCore.Mvc;
using ECommerceProject.Models;
using Microsoft.AspNetCore.Http; // Để dùng Session

namespace ECommerceProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. ĐĂNG KÝ ---
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Customer customer)
        {
            // Kiểm tra email đã tồn tại chưa
            var check = _context.Customers.FirstOrDefault(s => s.Email == customer.Email);
            if (check == null)
            {
                // --- SỬA: Gán mặc định là User để đảm bảo an toàn ---
                customer.Role = "User";
                // ---------------------------------------------------

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.Error = "Email này đã được sử dụng!";
                return View();
            }
        }

        // --- 2. ĐĂNG NHẬP ---
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Customers.FirstOrDefault(c => c.Email == email && c.PasswordHash == password);

            if (user != null)
            {
                // 1. Lưu thông tin cơ bản (Cho cả Admin và User)
                HttpContext.Session.SetString("CustomerId", user.CustomerId.ToString());
                string fullName = (user.LastName ?? "") + " " + (user.FirstName ?? "");
                HttpContext.Session.SetString("CustomerName", fullName.Trim());

                // --- SỬA: KIỂM TRA QUYỀN ADMIN ---
                if (user.Role == "Admin")
                {
                    // Cấp quyền Admin
                    HttpContext.Session.SetString("AdminRole", "Admin");
                    // Chuyển thẳng vào trang quản trị
                    return RedirectToAction("Index", "Admin");
                }
                // ---------------------------------

                // Nếu là User thường -> Về trang chủ bán hàng
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai email hoặc mật khẩu!";
            return View();
        }

        // --- 3. ĐĂNG XUẤT ---
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch Session (Cả Admin lẫn User đều bị out)
            return RedirectToAction("Index", "Home");
        }

        // --- 4. XEM HỒ SƠ (GET) ---
        public IActionResult Profile()
        {
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (customerIdStr == null) return RedirectToAction("Login");

            int customerId = int.Parse(customerIdStr);
            var customer = _context.Customers.Find(customerId);
            return View(customer);
        }

        // --- 5. CẬP NHẬT HỒ SƠ (POST) ---
        [HttpPost]
        public async Task<IActionResult> Profile(string firstName, string lastName, string address, string newPassword)
        {
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (customerIdStr == null) return RedirectToAction("Login");

            int customerId = int.Parse(customerIdStr);
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer != null)
            {
                customer.FirstName = firstName;
                customer.LastName = lastName;
                customer.Address = address;

                if (!string.IsNullOrEmpty(newPassword))
                {
                    customer.PasswordHash = newPassword;
                }

                _context.Update(customer);
                await _context.SaveChangesAsync();

                // Cập nhật lại tên trong Session
                HttpContext.Session.SetString("CustomerName", customer.LastName + " " + customer.FirstName);

                ViewBag.Success = "Cập nhật hồ sơ thành công!";
                return View(customer);
            }

            return NotFound();
        }
    }
}