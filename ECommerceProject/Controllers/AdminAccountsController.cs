using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;

namespace ECommerceProject.Controllers
{
    public class AdminAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Middleware kiểm tra quyền Admin
        private bool IsAdmin() => HttpContext.Session.GetString("AdminRole") == "Admin";

        // 1. Danh sách tài khoản
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");

            var users = await _context.Customers
                                .OrderByDescending(u => u.Role)
                                .ThenBy(u => u.CustomerId)
                                .ToListAsync();
            return View(users);
        }

        // 2. Xóa tài khoản
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");

            var user = await _context.Customers.FindAsync(id);
            if (user != null)
            {
                _context.Customers.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. Đổi quyền (Giữ nguyên như cũ)
        public async Task<IActionResult> ToggleRole(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");

            var user = await _context.Customers.FindAsync(id);
            if (user != null)
            {
                if (user.Role == "User") user.Role = "Admin";
                else user.Role = "User";

                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- CHỨC NĂNG MỚI: SỬA TÀI KHOẢN ---

        // 4. Trang Sửa (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            if (id == null) return NotFound();

            var user = await _context.Customers.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // 5. Xử lý Sửa (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string firstName, string lastName, string email, string address, string newPassword)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");

            var user = await _context.Customers.FindAsync(id);
            if (user == null) return NotFound();

            // Cập nhật thông tin
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.Address = address;

            // Nếu nhập mật khẩu mới thì cập nhật, bỏ trống thì giữ nguyên
            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = newPassword;
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}