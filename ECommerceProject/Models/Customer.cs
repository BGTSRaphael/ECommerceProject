using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceProject.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        // --- THÊM DÒNG NÀY ---
        [StringLength(20)]
        public string Role { get; set; } = "User"; // Mặc định là User
        // ---------------------

        // Navigation Properties (Giữ nguyên)
        public virtual ICollection<Cart> Carts { get; set; }
    }
}