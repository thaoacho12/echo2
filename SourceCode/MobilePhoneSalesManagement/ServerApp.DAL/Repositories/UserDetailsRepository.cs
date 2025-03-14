using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.DAL.Repositories
{
    public interface IUserDetailsRepository : IGenericRepository<UserDetails>
    {
    }

    public class UserDetailsRepository : GenericRepository<UserDetails>, IUserDetailsRepository
    {
        private readonly ShopDbContext _context;

        public UserDetailsRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }

    }

}
