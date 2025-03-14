using Microsoft.EntityFrameworkCore.Storage;
using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ShopDbContext _context;
        private IDbContextTransaction _currentTransaction;

        // Các repository cho từng thực thể
        private IGenericRepository<User>? _userRepository;
        private IGenericRepository<UserDetails>? _userDetailsRepository;
        private IGenericRepository<Brand>? _brandRepository;
        private IGenericRepository<Product>? _productRepository;
        private IGenericRepository<Cart>? _cartRepository;
        private IGenericRepository<Order>? _orderRepository;
        private IGenericRepository<OrderItem>? _orderItemRepository;
        private IGenericRepository<WishList>? _wishListRepository;
        private IGenericRepository<Review>? _reviewRepository;
        private IGenericRepository<SpecificationType>? _specificationTypeRepository;
        private IGenericRepository<ProductSpecification>? _productSpecificationRepository;
        public UnitOfWork(ShopDbContext context)
        {
            _context = context;
        }

        public ShopDbContext Context => _context;

        // Các repository của từng DbSet
        public IGenericRepository<User> UserRepository =>
            _userRepository ??= new GenericRepository<User>(_context);

        public IGenericRepository<UserDetails> UserDetailsRepository =>
            _userDetailsRepository ??= new GenericRepository<UserDetails>(_context);

        public IGenericRepository<Brand> BrandRepository =>
            _brandRepository ??= new GenericRepository<Brand>(_context);

        public IGenericRepository<Product> ProductRepository =>
            _productRepository ??= new GenericRepository<Product>(_context);

        public IGenericRepository<Cart> CartRepository =>
            _cartRepository ??= new GenericRepository<Cart>(_context);

        public IGenericRepository<Order> OrderRepository =>
            _orderRepository ??= new GenericRepository<Order>(_context);

        public IGenericRepository<OrderItem> OrderItemRepository =>
            _orderItemRepository ??= new GenericRepository<OrderItem>(_context);

        public IGenericRepository<WishList> WishListRepository =>
            _wishListRepository ??= new GenericRepository<WishList>(_context);

        public IGenericRepository<Review> ReviewRepository =>
            _reviewRepository ??= new GenericRepository<Review>(_context);
        public IGenericRepository<SpecificationType> SpecificationTypeRepository =>
            _specificationTypeRepository ??= new GenericRepository<SpecificationType>(_context);
        public IGenericRepository<ProductSpecification> ProductSpecificationRepository =>
            _productSpecificationRepository ??= new GenericRepository<ProductSpecification>(_context);

        // Phương thức Generic Repository
        public IGenericRepository<TEntity> GenericRepository<TEntity>() where TEntity : class
        {
            return new GenericRepository<TEntity>(_context);
        }

        // Lưu thay đổi trong context
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Quản lý transaction
        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        // Dispose
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task CommitAsync()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No transaction in progress.");

            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
        public async Task RollbackAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

}
