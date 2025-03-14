using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Repositories
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
    }

    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        private readonly ShopDbContext _context;

        public OrderItemRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }


    }

}
