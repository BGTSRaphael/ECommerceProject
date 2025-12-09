using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;

namespace ECommerceProject.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Thêm tham số 'page' vào hàm Index (Mặc định là trang 1)
        public async Task<IActionResult> Index(int? categoryId, string searchString, decimal? minPrice, decimal? maxPrice, string sortOrder, int page = 1)
        {
            // 1. Khởi tạo query
            var products = _context.Products.Include(p => p.Category).AsQueryable();
            var categories = await _context.Categories.ToListAsync();

            // 2. Áp dụng bộ lọc (Giữ nguyên logic cũ)
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString));
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // 3. Sắp xếp (Giữ nguyên logic cũ)
            switch (sortOrder)
            {
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                default:
                    products = products.OrderByDescending(p => p.ProductId);
                    break;
            }

            // --- 4. XỬ LÝ PHÂN TRANG (MỚI) ---
            int pageSize = 6; // Số sản phẩm trên 1 trang (Bạn có thể đổi thành 9 hoặc 12)

            // Đếm tổng số sản phẩm THỎA MÃN ĐIỀU KIỆN LỌC (Quan trọng)
            int totalItems = await products.CountAsync();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Cắt dữ liệu theo trang (Skip & Take)
            var pagedProducts = await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 5. Đóng gói ViewModel
            var viewModel = new ProductListViewModel
            {
                Products = pagedProducts, // Chỉ đưa danh sách đã cắt sang View
                Categories = categories,
                CurrentCategoryId = categoryId,
                SearchString = searchString,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                CurrentPage = page,      // Trang hiện tại
                TotalPages = totalPages  // Tổng số trang
            };

            return View(viewModel);
        }

        // ... (Các hàm khác giữ nguyên) ...
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}