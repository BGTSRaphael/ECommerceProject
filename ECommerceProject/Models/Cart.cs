using ECommerceProject.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceProject.Models
{
    [Table("Carts")]
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public DateTime DateCreated { get; set; }

        // --- THÊM DÒNG NÀY ---
        public int OrderStatus { get; set; } = 0; // 0: Chờ, 1: Giao, 2: Xong, 3: Hủy
        // ---------------------

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<CartDetail> CartDetails { get; set; }
    }
}
