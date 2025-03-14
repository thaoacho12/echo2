using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Repositories
{
    public interface IWishListRepository : IGenericRepository<WishList>
    {
    }

    public class WishListRepository : GenericRepository<WishList>, IWishListRepository
    {
        private readonly ShopDbContext _context;

        public WishListRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
