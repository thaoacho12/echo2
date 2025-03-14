using System.Linq.Expressions;

namespace ServerApp.BLL.Services.Base
{
    public interface IBaseService<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<int> AddAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(int id);

        int Add(T entity);
        int Update(T entity);
        int Delete(Guid id);
        int Delete(int id);
        Task<int> DeleteAsync(Guid id);
        Task<bool> DeleteAsync(T entity);

        IEnumerable<T> GetAll();

        T? GetById(Guid id);


        T? GetById(int id); // Thêm phương thức này
        Task<T?> GetByIdAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync(
             Expression<Func<T, bool>>? filter = null,
             Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
             string includesProperties = ""
         );
        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includesProperties = ""
        );
        void ValidateModelPropertiesWithAttribute<T>(T model);
        Task<(int countRemoved, int countUpdated)> DeleteMultipleAsync(
            IEnumerable<int> ids,
            Func<T, bool> condition,
            Func<T?, Task> onConditionFailed);
        Task<int> RestoreMultipleAsync(
         IEnumerable<int> ids,
         Func<T, bool> condition,
         Func<T?, Task> onCondition);
    }
}
