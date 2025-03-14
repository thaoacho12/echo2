using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
    }

    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly ShopDbContext _context;

        public ReviewRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }

    }

}
