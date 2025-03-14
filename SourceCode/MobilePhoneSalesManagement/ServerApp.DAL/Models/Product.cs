namespace ServerApp.DAL.Models
{
    public class Product
    {
        public int ProductId { get; set; } // Primary Key
        public string Name { get; set; } // Tên sản phẩm
        public string Description { get; set; } // Mô tả sản phẩm
        public decimal Price { get; set; } // Giá hiện tại
        public decimal OldPrice { get; set; } // Giá cũ
        public int StockQuantity { get; set; } = 0; // Số lượng trong kho
        public int? BrandId { get; set; } // Khóa ngoại liên kết với bảng Brand
        public string Manufacturer { get; set; } // Nhà sản xuất
        public bool IsActive { get; set; } = true; // Trạng thái hoạt động
        public string Colors { get; set; } // Màu sắc sản phẩm
        public int Discount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm tạo
        public DateTime UpdatedAt { get; set; } = DateTime.Now; // Thời điểm cập nhật

        // Navigation properties
        public virtual Brand Brand { get; set; } // Liên kết với bảng Brand
        public virtual ICollection<Cart> Carts { get; set; } // Liên kết với bảng Cart
        public virtual ICollection<OrderItem> OrderItems { get; set; } // Liên kết với bảng OrderItem
        public virtual ICollection<WishList> WishLists { get; set; } // Liên kết với bảng WishList
        public virtual ICollection<Review> Reviews { get; set; } // Liên kết với bảng Review
        public virtual ICollection<ProductSpecification> ProductSpecifications { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int? CategoryId { get; set; }
        public int? ImageId { get; set; }
        public virtual Image Image { get; set; }
    }
}
