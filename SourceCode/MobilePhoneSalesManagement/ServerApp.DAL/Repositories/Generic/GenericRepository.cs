using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ServerApp.DAL.Data;
using System.Linq.Expressions;

namespace ServerApp.DAL.Repositories.Generic
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ShopDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ShopDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Get by Id
        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Get all entities
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Add new entity
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // Update entity
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }
        
        // Delete entity by Id
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public void Delete(Guid id)
        {
            var entity = _dbSet.Find(id);

            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
        public void Delete(int id)
        {
            var entity = _dbSet.Find(id);

            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
        public void Delete(T entity)
        {

            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public IQueryable<T> Get(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includesProperties = "")
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includesProperties))
            {
                foreach (var property in includesProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query.Include(property);
                }
            }


            return orderBy != null ? orderBy(query) : query;
        }
        

        public T? GetById(Guid id)
        {
            return _dbSet.Find(id);
        }
        public T? GetById(int id)
        {
            return _dbSet.Find(id);
        }
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        

        public IQueryable<T> GetQuery(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate != null)
            {
                return await _dbSet.Where(predicate).ToListAsync();
            }
            return await _dbSet.ToListAsync();
        }



        /// <summary>
        /// Các phiên bản tùy chỉnh đã dùng
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> GetQuery()
        {
            return _dbSet;
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null
        )
        {
            IQueryable<T> query = _dbSet;

            // Áp dụng filter
            query = query.Where(filter);

            // Áp dụng include nếu có
            if (include != null)
            {
                query = include(query);
            }

            // Áp dụng orderBy nếu có
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Trả về kết quả đầu tiên hoặc null
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null
        )
        {
            IQueryable<T> query = _context.Set<T>();

            // Áp dụng filter nếu có
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Áp dụng include nếu có
            if (include != null)
            {
                query = include(query);
            }

            return query;
        }
        public async Task<int> ModifyAsync(T entity)
        {
            // Cập nhật thực thể
            _dbSet.Update(entity);

            // Lưu các thay đổi vào cơ sở dữ liệu
            int affectedRows = await _context.SaveChangesAsync();

            // Kiểm tra nếu có bản ghi nào bị ảnh hưởng, nghĩa là cập nhật thành công
            return affectedRows;
        }

    }


}
