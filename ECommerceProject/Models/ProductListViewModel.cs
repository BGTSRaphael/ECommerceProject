namespace ECommerceProject.Models
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public int? CurrentCategoryId { get; set; }
        public string? SearchString { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortOrder { get; set; }

        // --- THÊM 2 DÒNG NÀY CHO PHÂN TRANG ---
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}