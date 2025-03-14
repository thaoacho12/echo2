using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        // DbContext cho ứng dụng
        ShopDbContext Context { get; }

        // Các repository tương ứng với các DbSet trong DbContext
        IGenericRepository<User> UserRepository { get; }
        IGenericRepository<UserDetails> UserDetailsRepository { get; }
        IGenericRepository<Brand> BrandRepository { get; }
        IGenericRepository<Product> ProductRepository { get; }
        IGenericRepository<Cart> CartRepository { get; }
        IGenericRepository<Order> OrderRepository { get; }
        IGenericRepository<OrderItem> OrderItemRepository { get; }
        IGenericRepository<WishList> WishListRepository { get; }
        IGenericRepository<Review> ReviewRepository { get; }
        IGenericRepository<SpecificationType> SpecificationTypeRepository { get; }
        IGenericRepository<ProductSpecification> ProductSpecificationRepository { get; }

        // Phương thức Generic Repository
        IGenericRepository<TEntity> GenericRepository<TEntity>() where TEntity : class;

        // Các phương thức quản lý thay đổi trong cơ sở dữ liệu
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Quản lý các giao dịch (transactions)
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }

}
