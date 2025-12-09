using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;

namespace ECommerceProject.Controllers
{
    public class AdminProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment; // Để xử lý file ảnh

        public AdminProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Middleware kiểm tra đăng nhập
        private bool IsAdmin() => HttpContext.Session.GetString("AdminRole") == "Admin";

        // 1. Danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            var products = await _context.Products.Include(p => p.Category).OrderByDescending(p => p.ProductId).ToListAsync();
            return View(products);
        }

        // 2. Trang Thêm mới (GET)
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // 3. Xử lý Thêm mới (POST) - ĐÃ SỬA LỖI UPLOAD
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                // A. Xác định đường dẫn thư mục lưu ảnh
                string uploadDir = Path.Combine(_environment.WebRootPath, "images");

                // B. Kiểm tra: Nếu thư mục chưa có thì tạo mới (SỬA LỖI CỦA BẠN Ở ĐÂY)
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // C. Tạo tên file độc nhất (tránh trùng)
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadDir, filename);

                // D. Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.ImageUrl = filename;
            }
            else
            {
                product.ImageUrl = "default.jpg"; // Ảnh mặc định nếu không upload
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // (Tùy chọn) Xóa cả file ảnh cũ để dọn rác
                if (product.ImageUrl != "default.jpg")
                {
                    string oldPath = Path.Combine(_environment.WebRootPath, "images", product.ImageUrl);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    // 5. Trang Sửa sản phẩm (GET)
public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // 6. Xử lý Sửa sản phẩm (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (id != product.ProductId) return NotFound();

            // Lấy thông tin cũ để giữ lại ảnh nếu không upload ảnh mới
            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);

            if (imageFile != null)
            {
                // Nếu có upload ảnh mới -> Xử lý lưu ảnh như hàm Create
                string uploadDir = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                string filename = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadDir, filename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.ImageUrl = filename; // Cập nhật tên ảnh mới
            }
            else
            {
                // Nếu không chọn ảnh mới -> Giữ nguyên ảnh cũ
                product.ImageUrl = existingProduct.ImageUrl;
            }

            _context.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
