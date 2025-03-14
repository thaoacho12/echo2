using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServerApp.DAL.Models;
using ServerApp.DAL.Models.AuthenticationModels;

namespace ServerApp.DAL.Data
{
    public class ShopDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { }

        // DbSet for each table
        public DbSet<User> Users { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SpecificationType> SpecificationTypes { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Image> Images { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình UserId làm khóa chính
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId); // Đặt UserId làm khóa chính
                entity.Property(u => u.UserId).ValueGeneratedOnAdd(); // Tự động tăng
            });


            // Brand -> Product (SetNull)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);


            // Định nghĩa khóa chính cho bảng ProductSpecification
            modelBuilder.Entity<ProductSpecification>()
                .HasKey(ps => new { ps.ProductId, ps.SpecificationTypeId });
            // Product -> Specification (Cascade Delete)
            modelBuilder.Entity<ProductSpecification>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSpecifications)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductSpecification>()
                .HasOne(ps => ps.SpecificationType)
                .WithMany(st => st.ProductSpecifications)
                .HasForeignKey(ps => ps.SpecificationTypeId)
                .OnDelete(DeleteBehavior.Restrict); // hoặc DeleteBehavior.NoAction


            // Product -> Review (Cascade Delete)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // khóa chính cart
            modelBuilder.Entity<Cart>()
                .HasKey(c => new { c.UserId, c.ProductId });

            // khóa chính review
            modelBuilder.Entity<Review>()
                .HasKey(c => new { c.UserId, c.ProductId });

            // khóa chính WishList
            modelBuilder.Entity<WishList>()
                .HasKey(c => new { c.UserId, c.ProductId });

            // khóa chính OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasKey(c => new { c.OrderId, c.ProductId });


            // Mối quan hệ giữa Product và Image
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Image) // Một sản phẩm có một hình ảnh
                .WithMany()  // Không cần phải tạo mối quan hệ ngược lại trong Image
                .HasForeignKey(p => p.ImageId); // Khóa ngoại

            // Mối quan hệ giữa Brand và Image
            modelBuilder.Entity<Brand>()
                .HasOne(b => b.Image) // Một thương hiệu có một hình ảnh
                .WithMany()  // Không cần phải tạo mối quan hệ ngược lại trong Image
                .HasForeignKey(b => b.ImageId); // Khóa ngoại
        }

    }
}
