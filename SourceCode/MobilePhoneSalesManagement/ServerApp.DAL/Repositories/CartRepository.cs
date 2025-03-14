using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
    }

    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly ShopDbContext _context;

        public CartRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }

    }

}
