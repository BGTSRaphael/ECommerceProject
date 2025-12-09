using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần để dùng Skip, Take, ToListAsync
using ECommerceProject.Models;

namespace ECommerceProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Cập nhật hàm Index để nhận tham số 'page'
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 8; // Số sản phẩm trên trang chủ (Ví dụ: 2 hàng x 4 cột)

            // 1. Truy vấn dữ liệu
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // 2. Tính toán phân trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 3. Lấy dữ liệu theo trang
            // Sắp xếp sản phẩm mới nhất lên đầu trang chủ
            var products = await query
                                .OrderByDescending(p => p.ProductId)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            // 4. Đóng gói vào ViewModel (Tái sử dụng ProductListViewModel)
            var viewModel = new ProductListViewModel
            {
                Products = products,
                CurrentPage = page,
                TotalPages = totalPages
                // Các thuộc tính lọc khác (Search, Category...) để null vì trang chủ không cần lọc phức tạp
            };

            return View(viewModel);
        }

        // ... Các hàm Privacy, Error giữ nguyên
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}