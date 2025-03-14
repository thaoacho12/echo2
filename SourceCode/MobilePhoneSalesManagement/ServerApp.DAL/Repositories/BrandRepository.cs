using Microsoft.EntityFrameworkCore;
using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;
using System;

namespace ServerApp.DAL.Repositories
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
    }

    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        private readonly ShopDbContext _context;

        public BrandRepository(ShopDbContext context) : base(context)
        {
            _context = context;
        }

    }

}
