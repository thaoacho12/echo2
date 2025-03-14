using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
    }

    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ShopDbContext _context;

        public ProductRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }

    }

}
