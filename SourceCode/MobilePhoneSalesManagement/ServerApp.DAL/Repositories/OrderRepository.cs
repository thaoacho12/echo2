using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
    }

    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ShopDbContext _context;

        public OrderRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }

    }

}
