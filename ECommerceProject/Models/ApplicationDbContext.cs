using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartDetail> CartDetails { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Cấu hình Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            });

            // 2. Cấu hình Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                // Các thuộc tính khác tự động map theo tên
            });

            // 3. Cấu hình Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Product_Category");
            });

            // 4. Cấu hình Cart (ĐÂY LÀ CHỖ SỬA LỖI)
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.CartId);

                //entity.HasIndex(e => e.CustomerId).IsUnique(); // Giữ Unique Index từ SQL

                // SỬA: Đổi WithOne(...) thành WithMany(...) để khớp với ICollection<Cart> Carts trong Customer.cs
                // Và xóa <Cart> khỏi HasForeignKey
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Carts)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cart_Customer");
            });

            // 5. Cấu hình CartDetail
            modelBuilder.Entity<CartDetail>(entity =>
            {
                entity.ToTable("Cart_Detail"); // Map đúng tên bảng SQL (có dấu gạch dưới)
                entity.HasKey(e => e.CartDetailId);

                entity.HasOne(d => d.Cart)
                    .WithMany(p => p.CartDetails)
                    .HasForeignKey(d => d.CartId)
                    .HasConstraintName("FK_CartDetail_Cart");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.CartDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_CartDetail_Product");
            });
        }
    }
}