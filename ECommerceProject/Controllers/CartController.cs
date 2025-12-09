using Microsoft.AspNetCore.Mvc;
using ECommerceProject.Models;
using ECommerceProject.Helpers; // Để dùng Session Extensions
using Microsoft.AspNetCore.Http; // Để dùng HttpContext.Session

namespace ECommerceProject.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Xem giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // 2. Thêm sản phẩm vào giỏ
        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var check = cart.FirstOrDefault(x => x.ProductId == id);

            if (check == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = 1
                });
            }
            else
            {
                check.Quantity++;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        // 3. Xóa sản phẩm khỏi giỏ
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        // 4. Trang Thanh toán (GET) - YÊU CẦU LOGIN
        [HttpGet]
        public IActionResult Checkout()
        {
            // KIỂM TRA ĐĂNG NHẬP: Nếu chưa có Session CustomerId -> Đá về trang Login
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            // (Tùy chọn) Lấy thông tin User để hiển thị nếu cần
            var customerId = int.Parse(HttpContext.Session.GetString("CustomerId"));
            var user = _context.Customers.Find(customerId);
            ViewBag.CurrentUser = user;

            return View(cart);
        }

        // 5. Xử lý Thanh toán (POST) - LƯU VÀO DB VỚI ID NGƯỜI DÙNG
        [HttpPost]
        public async Task<IActionResult> Checkout(string address)
        {
            // 1. Kiểm tra lại Session cho chắc chắn
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (customerIdStr == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            int customerId = int.Parse(customerIdStr);

            // 2. Tạo đơn hàng (Cart) gắn với CustomerId hiện tại
            var newCart = new Cart
            {
                CustomerId = customerId,
                DateCreated = DateTime.Now,
                OrderStatus = 0 // --- THÊM DÒNG NÀY (Mặc định là 0: Chờ xác nhận) ---
            };

            _context.Carts.Add(newCart);
            await _context.SaveChangesAsync(); // Lưu xong mới có CartId

            // 3. Lưu chi tiết đơn hàng
            foreach (var item in cart)
            {
                var cartDetail = new CartDetail
                {
                    CartId = newCart.CartId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtTime = item.Price
                };
                _context.CartDetails.Add(cartDetail);
            }
            await _context.SaveChangesAsync();

            // 4. Xóa giỏ hàng sau khi mua thành công
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderSuccess");
        }

        // 6. Trang báo thành công
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}